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