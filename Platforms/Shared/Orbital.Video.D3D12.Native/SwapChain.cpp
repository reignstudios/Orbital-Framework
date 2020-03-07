#include "SwapChain.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT SwapChain* Orbital_Video_D3D12_SwapChain_Create(Device* device)
	{
		SwapChain* handle = (SwapChain*)calloc(1, sizeof(SwapChain));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_SwapChain_Init(SwapChain* handle, HWND hWnd, UINT width, UINT height, UINT bufferCount, int fullscreen, SwapChainFormat format)
	{
		handle->bufferCount = bufferCount;
		if (!GetNative_SwapChainFormat(format, &handle->format)) return false;

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
		swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT::DXGI_SWAP_EFFECT_FLIP_DISCARD;
		swapChainDesc.SampleDesc.Count = 1;// swap-chains do not support msaa
		swapChainDesc.SampleDesc.Quality = 0;

		DXGI_SWAP_CHAIN_FULLSCREEN_DESC fullscreenDesc = {};
		fullscreenDesc.Windowed = fullscreen == 0;
		fullscreenDesc.RefreshRate.Numerator = 0;
		fullscreenDesc.RefreshRate.Denominator = 0;
		fullscreenDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER::DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
		fullscreenDesc.Scaling = DXGI_MODE_SCALING::DXGI_MODE_SCALING_UNSPECIFIED;

		IDXGISwapChain1* swapChain = NULL;
		if (FAILED(handle->device->instance->factory->CreateSwapChainForHwnd(handle->device->commandQueue, hWnd, &swapChainDesc, &fullscreenDesc, NULL, &swapChain))) return 0;
		handle->swapChain = (IDXGISwapChain3*)swapChain;
		if (FAILED(handle->device->instance->factory->MakeWindowAssociation(hWnd, DXGI_MWA_NO_ALT_ENTER))) return 0;
		handle->resourceState = D3D12_RESOURCE_STATE_PRESENT;// set default state

		// create render targets views
		D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
		rtvHeapDesc.NumDescriptors = bufferCount;
		rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
		rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
		if (FAILED(handle->device->device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&handle->resourceHeap)))) return 0;
		UINT resourceHeapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

		D3D12_CPU_DESCRIPTOR_HANDLE resourceDescCPUHandle = handle->resourceHeap->GetCPUDescriptorHandleForHeapStart();
		handle->resourceDescCPUHandles = (D3D12_CPU_DESCRIPTOR_HANDLE*)calloc(bufferCount, sizeof(D3D12_CPU_DESCRIPTOR_HANDLE));
		handle->resources = (ID3D12Resource**)calloc(bufferCount, sizeof(ID3D12Resource*));
		for (UINT i = 0; i != bufferCount; ++i)
        {
            if (FAILED(handle->swapChain->GetBuffer(i, IID_PPV_ARGS(&handle->resources[i])))) return 0;
            handle->device->device->CreateRenderTargetView(handle->resources[i], nullptr, resourceDescCPUHandle);
			handle->resourceDescCPUHandles[i] = resourceDescCPUHandle;
            resourceDescCPUHandle.ptr += resourceHeapSize;
        }

		// create helpers for synchronous buffer operations
		if (FAILED(handle->device->device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, handle->device->commandAllocator, nullptr, IID_PPV_ARGS(&handle->internalCommandList)))) return 0;
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
		if (handle->resourceDescCPUHandles != NULL)
		{
			free(handle->resourceDescCPUHandles);
			handle->resourceDescCPUHandles = NULL;
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

		if (handle->resourceHeap != NULL)
		{
			handle->resourceHeap->Release();
			handle->resourceHeap = NULL;
		}

		if (handle->swapChain != NULL)
		{
			handle->swapChain->Release();
			handle->swapChain = NULL;
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

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_BeginFrame(SwapChain* handle)
	{
		handle->currentRenderTargetIndex = handle->swapChain->GetCurrentBackBufferIndex();
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_Present(SwapChain* handle)
	{
		if (handle->resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT)
		{
			// reset command list and copy resource
			handle->internalCommandList->Reset(handle->device->commandAllocator, NULL);

			// change resource state to present
			D3D12_RESOURCE_BARRIER barrier = {};
			barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
			barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
			barrier.Transition.pResource = handle->resources[handle->currentRenderTargetIndex];
			barrier.Transition.StateBefore = handle->resourceState;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->resourceState = barrier.Transition.StateAfter;
			handle->internalCommandList->ResourceBarrier(1, &barrier);

			// close command list
			handle->internalCommandList->Close();

			// execute operations
			ID3D12CommandList* commandLists[1] = { handle->internalCommandList };
			handle->device->commandQueue->ExecuteCommandLists(1, commandLists);
			WaitForFence(handle->device, handle->internalFence, handle->internalFenceEvent, handle->internalFenceValue);
		}
		handle->swapChain->Present(1, 0);
	}
}

void Orbital_Video_D3D12_SwapChain_ChangeState(SwapChain* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList)
{
	if (handle->resourceState == state) return;
	D3D12_RESOURCE_BARRIER barrier = {};
	barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
	barrier.Transition.pResource = handle->resources[handle->currentRenderTargetIndex];
	barrier.Transition.StateBefore = handle->resourceState;
	barrier.Transition.StateAfter = state;
	barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
	commandList->ResourceBarrier(1, &barrier);
	handle->resourceState = state;
}