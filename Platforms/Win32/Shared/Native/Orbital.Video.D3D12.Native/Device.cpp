#include "pch.h"
#include "Device.h"
#include "D3D12_Extern.h"

bool Device::Init(FeatureLevel featureLevel, bool softwareRasterizer)
{
	// get native feature level
	D3D_FEATURE_LEVEL nativeFeatureLevel;
	switch (featureLevel)
	{
		case FeatureLevel::Level_9_1: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_1; break;
		case FeatureLevel::Level_9_2: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_2; break;
		case FeatureLevel::Level_9_3: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_3; break;
		case FeatureLevel::Level_10_0: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_0; break;
		case FeatureLevel::Level_10_1: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_1; break;
		case FeatureLevel::Level_11_0: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_0; break;
		case FeatureLevel::Level_11_1: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_1; break;
		case FeatureLevel::Level_12_0: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_0; break;
		case FeatureLevel::Level_12_1: nativeFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_1; break;
		default: return false;
	}

	// enable debugging
	UINT factoryFlags = 0;
#if defined(_DEBUG)
	if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
	{
		debugController->EnableDebugLayer();
		factoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
	}
#endif

	if (FAILED(CreateDXGIFactory2(factoryFlags, IID_PPV_ARGS(&factory)))) return false;

	// get adapter
	if (softwareRasterizer)
	{
		if (FAILED(factory->EnumWarpAdapter(IID_PPV_ARGS(&adapter)))) return false;
	}
	else
	{
		IDXGIAdapter1* adapter1 = 0;
		for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != factory->EnumAdapters1(adapterIndex, &adapter1); ++adapterIndex)
		{
			DXGI_ADAPTER_DESC1 desc;
			if (FAILED(adapter1->GetDesc1(&desc)))
			{
				adapter1->Release();
				return false;
			}

			if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
			{
				adapter1->Release();
				continue;
			}

			if (SUCCEEDED(D3D12CreateDevice(adapter1, nativeFeatureLevel, _uuidof(ID3D12Device), nullptr)))
			{
				adapter = adapter1;
				break;
			}

			adapter1->Release();
		}
	}

	if (adapter == NULL) return false;

	// create device
	if (FAILED(D3D12CreateDevice(adapter, nativeFeatureLevel, IID_PPV_ARGS(&device)))) return false;

	// create command queue
	D3D12_COMMAND_QUEUE_DESC queueDesc = {};
	queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
	queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
	if (FAILED(device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&commandQueue)))) return false;

	return true;
}

void Device::Dispose()
{
	if (commandQueue != NULL)
	{
		commandQueue->Release();
		commandQueue = NULL;
	}

	if (device != NULL)
	{
		device->Release();
		device = NULL;
	}

	if (adapter != NULL)
	{
		adapter->Release();
		adapter = NULL;
	}

	if (factory != NULL)
	{
		factory->Release();
		factory = NULL;
	}

	if (debugController != NULL)
	{
		debugController->Release();
		debugController = NULL;
	}
}

extern "C"
{
	D3D12_EXTERN Device* Orbital_Video_D3D12_Device_Create()
	{
		return (Device*)calloc(1, sizeof(Device));
	}

	D3D12_EXTERN bool Orbital_Video_D3D12_Device_Init(Device* device, FeatureLevel featureLevel, bool softwareRasterizer)
	{
		return device->Init(featureLevel, softwareRasterizer);
	}

	D3D12_EXTERN void Orbital_Video_D3D12_Device_Dispose(Device* device)
	{
		device->Dispose();
		free(device);
	}
}