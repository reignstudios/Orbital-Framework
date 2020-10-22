#include "SwapChain.h"
#include "Utils.h"
#include "Texture.h"

extern "C"
{
	ORBITAL_EXPORT SwapChain* Orbital_Video_D3D12_SwapChain_Create(Device* device, SwapChainType type)
	{
		SwapChain* handle = (SwapChain*)calloc(1, sizeof(SwapChain));
		handle->device = device;
		handle->type = type;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_SwapChain_Init(SwapChain* handle, HWND hWnd, UINT width, UINT height, UINT bufferCount, int fullscreen, SwapChainFormat format, SwapChainVSyncMode vSyncMode)
	{
		if (handle->type == SwapChainType::SwapChainType_MultiGPU_AFR)
		{
			handle->nodeCount = handle->device->nodeCount;
			bufferCount = handle->device->nodeCount;// if multi-gpu buffer count matches GPU count
		}
		else
		{
			handle->nodeCount = 1;// if type is single-gpu force to single node
		}
		handle->bufferCount = bufferCount;
		if (!GetNative_SwapChainFormat(format, &handle->format)) return false;
		handle->vSyncMode = vSyncMode;
		handle->vSync = (vSyncMode == SwapChainVSyncMode_VSyncOn) ? 1 : 0;
		handle->fullscreen = fullscreen == 1 ? true : false;

		// check format support
		D3D12_FEATURE_DATA_FORMAT_INFO formatInfo = {};
		formatInfo.Format = handle->format;
		if (FAILED(handle->device->device->CheckFeatureSupport(D3D12_FEATURE_FORMAT_INFO, &formatInfo, sizeof(D3D12_FEATURE_DATA_FORMAT_INFO)))) return 0;

		// create swap-chain
		DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
		swapChainDesc.BufferCount = bufferCount;
		swapChainDesc.Width = width;
		swapChainDesc.Height = height;
		swapChainDesc.Format = handle->format;
		swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
		swapChainDesc.SampleDesc.Count = 1;// swap-chains do not support msaa
		swapChainDesc.SampleDesc.Quality = 0;
		swapChainDesc.Flags = (vSyncMode == SwapChainVSyncMode_VSyncOff && !fullscreen) ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

		DXGI_SWAP_CHAIN_FULLSCREEN_DESC fullscreenDesc = {};
		fullscreenDesc.Windowed = fullscreen == 0;
		fullscreenDesc.RefreshRate.Numerator = 0;
		fullscreenDesc.RefreshRate.Denominator = 0;
		fullscreenDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER::DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
		fullscreenDesc.Scaling = DXGI_MODE_SCALING::DXGI_MODE_SCALING_UNSPECIFIED;

		DeviceNode* primaryDeviceNode = &handle->device->nodes[0];// always use primary device node to create swap-chain
		if (FAILED(handle->device->instance->factory->CreateSwapChainForHwnd(primaryDeviceNode->commandQueue, hWnd, &swapChainDesc, &fullscreenDesc, NULL, &handle->swapChain1))) return 0;
		//handle->device->instance->factory->CreateSwapChain()
		handle->swapChain = handle->swapChain1;
		if (FAILED(handle->swapChain->QueryInterface(&handle->swapChain3))) return 0;
		if (FAILED(handle->device->instance->factory->MakeWindowAssociation(hWnd, DXGI_MWA_NO_ALT_ENTER))) return 0;

		// create resource object arrays
		handle->resources = (ID3D12Resource**)calloc(bufferCount, sizeof(ID3D12Resource*));
		handle->resourceDescCPUHandles = (D3D12_CPU_DESCRIPTOR_HANDLE*)calloc(bufferCount, sizeof(D3D12_CPU_DESCRIPTOR_HANDLE));
		handle->resourceStates = (D3D12_RESOURCE_STATES*)calloc(bufferCount, sizeof(D3D12_RESOURCE_STATES));
		for (UINT i = 0; i != bufferCount; ++i)
		{
			handle->resourceStates[i] = D3D12_RESOURCE_STATE_PRESENT;// set default state
		}

		// create nodes
		handle->nodes = (SwapChainNode*)calloc(handle->nodeCount, sizeof(SwapChainNode));
		for (UINT n = 0; n != handle->nodeCount; ++n)
		{
			// create memory heaps
			if (handle->type == SwapChainType::SwapChainType_SingleGPU_Standard)// single GPU standard swap buffers
			{
				// create render targets views
				D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
				rtvHeapDesc.NodeMask = primaryDeviceNode->mask;
				rtvHeapDesc.NumDescriptors = bufferCount;
				rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
				rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
				if (FAILED(handle->device->device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&handle->nodes[n].resourceHeap)))) return 0;// only one resource heap for single GPU
				UINT resourceHeapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

				D3D12_CPU_DESCRIPTOR_HANDLE resourceDescCPUHandle = handle->nodes[n].resourceHeap->GetCPUDescriptorHandleForHeapStart();
				for (UINT i = 0; i != bufferCount; ++i)
				{
					if (FAILED(handle->swapChain->GetBuffer(i, IID_PPV_ARGS(&handle->resources[i])))) return 0;
					handle->device->device->CreateRenderTargetView(handle->resources[i], nullptr, resourceDescCPUHandle);
					handle->resourceDescCPUHandles[i] = resourceDescCPUHandle;
					resourceDescCPUHandle.ptr += resourceHeapSize;
				}
			}
			else if (handle->type == SwapChainType::SwapChainType_MultiGPU_AFR)// multi GPU AFR buffers
			{
				// create render targets views
				D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
				rtvHeapDesc.NodeMask = handle->device->nodes[n].mask;
				rtvHeapDesc.NumDescriptors = 1;
				rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
				rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
				if (FAILED(handle->device->device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&handle->nodes[n].resourceHeap)))) return 0;

				D3D12_CPU_DESCRIPTOR_HANDLE resourceDescCPUHandle = handle->nodes[n].resourceHeap->GetCPUDescriptorHandleForHeapStart();
				if (FAILED(handle->swapChain->GetBuffer(n, IID_PPV_ARGS(&handle->resources[n])))) return 0;
				handle->device->device->CreateRenderTargetView(handle->resources[n], nullptr, resourceDescCPUHandle);
				handle->resourceDescCPUHandles[n] = resourceDescCPUHandle;
			}
			else
			{
				return 0;
			}
		}

		// create helpers for synchronous buffer operations
		if (FAILED(handle->device->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&handle->internalCommandAllocator)))) return 0;

		if (FAILED(handle->device->device->CreateCommandList(primaryDeviceNode->mask, D3D12_COMMAND_LIST_TYPE_DIRECT, handle->internalCommandAllocator, nullptr, IID_PPV_ARGS(&handle->internalCommandList)))) return 0;
		if (FAILED(handle->internalCommandList->Close())) return 0;// make sure this is closed as it defaults to open for writing

		if (FAILED(handle->device->device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&handle->internalFence)))) return 0;
		handle->internalFenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
		if (handle->internalFenceEvent == NULL) return 0;

		// make sure fence values start at 1 so they don't match 'GetCompletedValue' when its first called
		handle->internalFenceValue = 1;

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_Dispose(SwapChain* handle)
	{
		if (handle->nodes != NULL)
		{
			for (UINT n = 0; n != handle->nodeCount; ++n)
			{
				for (UINT n = 0; n != handle->nodeCount; ++n)
				{
					if (handle->nodes[n].resourceHeap != NULL)
					{
						handle->nodes[n].resourceHeap->Release();
						handle->nodes[n].resourceHeap = NULL;
					}
				}
			}

			free(handle->nodes);
			handle->nodes = NULL;
		}
		
		// dispose present helpers
		if (handle->internalFenceEvent != NULL)
		{
			CloseHandle(handle->internalFenceEvent);
			handle->internalFenceEvent = NULL;
		}

		if (handle->internalFence != NULL)
		{
			handle->internalFence->Release();
			handle->internalFence = NULL;
		}

		if (handle->internalCommandList != NULL)
		{
			handle->internalCommandList->Release();
			handle->internalCommandList = NULL;
		}

		if (handle->internalCommandAllocator != NULL)
		{
			handle->internalCommandAllocator->Release();
			handle->internalCommandAllocator = NULL;
		}

		// dispose main
		if (handle->resourceDescCPUHandles != NULL)
		{
			free(handle->resourceDescCPUHandles);
			handle->resourceDescCPUHandles = NULL;
		}

		if (handle->resourceStates != NULL)
		{
			free(handle->resourceStates);
			handle->resourceStates = NULL;
		}

		if (handle->resources != NULL)
		{
			for (UINT i = 0; i != handle->bufferCount; ++i)
			{
				if (handle->resources[i] != NULL)
				{
					handle->resources[i]->Release();
					handle->resources[i] = NULL;
				}
			}
			handle->resources = NULL;
		}

		if (handle->swapChain3 != NULL)
		{
			handle->swapChain3->Release();
			handle->swapChain3 = NULL;
		}

		if (handle->swapChain != NULL)
		{
			handle->swapChain->Release();
			handle->swapChain = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_BeginFrame(SwapChain* handle, int* currentNodeIndex, int* lastNodeIndex)
	{
		*lastNodeIndex = handle->currentNodeIndex;
		handle->currentRenderTargetIndex = handle->swapChain3->GetCurrentBackBufferIndex();
		if (handle->nodeCount == 1) handle->currentNodeIndex = 0;
		else handle->currentNodeIndex = handle->currentRenderTargetIndex;
		*currentNodeIndex = handle->currentNodeIndex;

		// reset command list and copy resource
		handle->internalCommandAllocator->Reset();
		DeviceNode* primaryDeviceNode = &handle->device->nodes[0];
		handle->internalCommandList->Reset(handle->internalCommandAllocator, NULL);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_Present(SwapChain* handle)
	{
		// make sure swap-chain surface is in present state
		UINT currentNodeIndex = handle->currentNodeIndex;
		DeviceNode* primaryDeviceNode = &handle->device->nodes[0];
		Orbital_Video_D3D12_SwapChain_ChangeState(handle, currentNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT, handle->internalCommandList);

		// close command list
		handle->internalCommandList->Close();

		// execute operations
		ID3D12CommandList* commandLists[1] = { handle->internalCommandList };
		primaryDeviceNode->commandQueue->ExecuteCommandLists(1, commandLists);
		WaitForFence_CommandQueue(primaryDeviceNode->commandQueue, handle->internalFence, handle->internalFenceEvent, handle->internalFenceValue);

		// preset frame
		UINT presentFlags = 0;
		if (handle->vSyncMode == SwapChainVSyncMode_VSyncOff && !handle->fullscreen) presentFlags |= DXGI_PRESENT_ALLOW_TEARING;
		handle->swapChain->Present(handle->vSync, presentFlags);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_ResolveRenderTexture(SwapChain* handle, Texture* srcRenderTexture)
	{
		UINT activeNodeIndex = handle->currentNodeIndex;
		Orbital_Video_D3D12_Texture_ChangeState(srcRenderTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_SOURCE, handle->internalCommandList);
		Orbital_Video_D3D12_SwapChain_ChangeState(handle, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_DEST, handle->internalCommandList);
		handle->internalCommandList->ResolveSubresource(handle->resources[handle->currentRenderTargetIndex], 0, srcRenderTexture->nodes[activeNodeIndex].resource, 0, srcRenderTexture->format);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_CopyTexture(SwapChain* handle, Texture* srcTexture)
	{
		UINT activeNodeIndex = handle->currentNodeIndex;
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->internalCommandList);
		Orbital_Video_D3D12_SwapChain_ChangeState(handle, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->internalCommandList);
		handle->internalCommandList->CopyResource(handle->resources[handle->currentRenderTargetIndex], srcTexture->nodes[activeNodeIndex].resource);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_CopyTextureRegion(SwapChain* handle, Texture* srcTexture, int srcX, int srcY, int dstX, int dstY, int width, int height, int srcMipmapLevel)
	{
		UINT activeNodeIndex = handle->currentNodeIndex;
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->internalCommandList);
		Orbital_Video_D3D12_SwapChain_ChangeState(handle, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->internalCommandList);

		D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
		dstLoc.pResource = handle->resources[handle->currentRenderTargetIndex];

		D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
		srcLoc.pResource = srcTexture->nodes[activeNodeIndex].resource;
		srcLoc.SubresourceIndex = srcMipmapLevel;

		D3D12_BOX srcBox;
		srcBox.left = srcX;
		srcBox.right = srcX + width;
		srcBox.top = srcY;
		srcBox.bottom = srcY + height;
		srcBox.front = 0;
		srcBox.back = 0;

		handle->internalCommandList->CopyTextureRegion(&dstLoc, dstX, dstY, 0, &srcLoc, &srcBox);
	}
}

void Orbital_Video_D3D12_SwapChain_ChangeState(SwapChain* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList)
{
	SwapChainNode* activeNode = &handle->nodes[nodeIndex];
	UINT currentRenderTargetIndex = handle->currentRenderTargetIndex;
	if (handle->resourceStates[currentRenderTargetIndex] == state) return;
	D3D12_RESOURCE_BARRIER barrier = {};
	barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
	barrier.Transition.pResource = handle->resources[handle->currentRenderTargetIndex];
	barrier.Transition.StateBefore = handle->resourceStates[currentRenderTargetIndex];
	barrier.Transition.StateAfter = state;
	barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
	commandList->ResourceBarrier(1, &barrier);
	handle->resourceStates[currentRenderTargetIndex] = state;
}