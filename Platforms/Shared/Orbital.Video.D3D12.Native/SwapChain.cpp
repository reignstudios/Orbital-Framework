#include "SwapChain.h"

extern "C"
{
	ORBITAL_EXPORT SwapChain* Orbital_Video_D3D12_SwapChain_Create(Device* device)
	{
		SwapChain* handle = (SwapChain*)calloc(1, sizeof(SwapChain));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_SwapChain_Init(SwapChain* handle, HWND hWnd, UINT width, UINT height, UINT bufferCount, int fullscreen)
	{
		handle->bufferCount = bufferCount;

		// create swap-chain
		DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
		swapChainDesc.BufferCount = bufferCount;
		swapChainDesc.Width = width;
		swapChainDesc.Height = height;
		swapChainDesc.Format = DXGI_FORMAT::DXGI_FORMAT_R8G8B8A8_UNORM;
		swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT::DXGI_SWAP_EFFECT_FLIP_DISCARD;
		swapChainDesc.SampleDesc.Count = 1;

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

		// create render targets views
		D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
		rtvHeapDesc.NumDescriptors = bufferCount;
		rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
		rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
		if (FAILED(handle->device->physicalDevice->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&handle->renderTargetViewHeap)))) return 0;
		UINT renderTargetViewHeapSize = handle->device->physicalDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);

		D3D12_CPU_DESCRIPTOR_HANDLE renderTargetDescHandle = handle->renderTargetViewHeap->GetCPUDescriptorHandleForHeapStart();
		handle->renderTargetDescHandles = (D3D12_CPU_DESCRIPTOR_HANDLE*)calloc(bufferCount, sizeof(D3D12_CPU_DESCRIPTOR_HANDLE));
		handle->renderTargetViews = (ID3D12Resource**)calloc(bufferCount, sizeof(ID3D12Resource*));
		for (UINT i = 0; i != bufferCount; ++i)
        {
            if (FAILED(handle->swapChain->GetBuffer(i, IID_PPV_ARGS(&handle->renderTargetViews[i])))) return 0;
            handle->device->physicalDevice->CreateRenderTargetView(handle->renderTargetViews[i], nullptr, renderTargetDescHandle);
			handle->renderTargetDescHandles[i] = renderTargetDescHandle;
            renderTargetDescHandle.ptr += renderTargetViewHeapSize;
        }

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_Dispose(SwapChain* handle)
	{
		if (handle->renderTargetDescHandles != NULL)
		{
			free(handle->renderTargetDescHandles);
			handle->renderTargetDescHandles = NULL;
		}

		if (handle->renderTargetViews != NULL)
		{
			for (UINT i = 0; i != handle->bufferCount; ++i)
			{
				if (handle->renderTargetViews[i] != NULL)
				{
					handle->renderTargetViews[i]->Release();
					handle->renderTargetViews[i] = NULL;
				}
			}
			handle->renderTargetViews = NULL;
		}

		if (handle->renderTargetViewHeap != NULL)
		{
			handle->renderTargetViewHeap->Release();
			handle->renderTargetViewHeap = NULL;
		}

		if (handle->swapChain != NULL)
		{
			handle->swapChain->Release();
			handle->swapChain = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_BeginFrame(SwapChain* handle)
	{
		handle->currentRenderTargetIndex = handle->swapChain->GetCurrentBackBufferIndex();
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_SwapChain_Present(SwapChain* handle)
	{
		handle->swapChain->Present(1, 0);
	}
}