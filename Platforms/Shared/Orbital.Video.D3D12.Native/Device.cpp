#include "pch.h"
#include "Device.h"
#include "D3D12_Extern.h"

extern "C"
{
	D3D12_EXTERN Device* Orbital_Video_D3D12_Device_Create()
	{
		return (Device*)calloc(1, sizeof(Device));
	}

	D3D12_EXTERN bool Orbital_Video_D3D12_Device_Init(Device* device, int adapterIndex, FeatureLevel minimumFeatureLevel, bool softwareRasterizer)
	{
		// get native feature level
		D3D_FEATURE_LEVEL nativeMinFeatureLevel;
		switch (minimumFeatureLevel)
		{
			case FeatureLevel::Level_9_1: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_1; break;
			case FeatureLevel::Level_9_2: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_2; break;
			case FeatureLevel::Level_9_3: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_3; break;
			case FeatureLevel::Level_10_0: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_0; break;
			case FeatureLevel::Level_10_1: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_1; break;
			case FeatureLevel::Level_11_0: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_0; break;
			case FeatureLevel::Level_11_1: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_1; break;
			case FeatureLevel::Level_12_0: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_0; break;
			case FeatureLevel::Level_12_1: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_1; break;
			default: return false;
		}

		// enable debugging
		UINT factoryFlags = 0;
		#if defined(_DEBUG)
		if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&device->debugController))))
		{
			device->debugController->EnableDebugLayer();
			factoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
		}
		#endif

		if (FAILED(CreateDXGIFactory2(factoryFlags, IID_PPV_ARGS(&device->factory)))) return false;

		// get adapter
		if (softwareRasterizer)
		{
			if (FAILED(device->factory->EnumWarpAdapter(IID_PPV_ARGS(&device->adapter)))) return false;
		}
		else
		{
			IDXGIAdapter1* adapter1 = NULL;
			if (adapterIndex != -1)
			{
				for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != device->factory->EnumAdapters1(adapterIndex, &adapter1); ++adapterIndex)
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

					if (SUCCEEDED(D3D12CreateDevice(adapter1, nativeMinFeatureLevel, _uuidof(ID3D12Device), nullptr)))
					{
						device->adapter = adapter1;
						break;
					}

					adapter1->Release();
				}

				if (device->adapter == NULL) return false;
			}
		}

		// create device
		if (FAILED(D3D12CreateDevice(device->adapter, nativeMinFeatureLevel, IID_PPV_ARGS(&device->device)))) return false;

		// get max feature level
		D3D_FEATURE_LEVEL supportedFeatureLevels[9] =
		{
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_1,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_2,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_9_3,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_0,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_10_1,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_1,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_0,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_1
		};
		D3D12_FEATURE_DATA_FEATURE_LEVELS featureLevelInfo = {};
		featureLevelInfo.NumFeatureLevels = 9;
		featureLevelInfo.pFeatureLevelsRequested = supportedFeatureLevels;
		if (FAILED(device->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_FEATURE_LEVELS, &featureLevelInfo, sizeof(D3D12_FEATURE_DATA_FEATURE_LEVELS)))) return false;

		// validate max isn't less than min
		if (featureLevelInfo.MaxSupportedFeatureLevel < nativeMinFeatureLevel) return false;

		// create command queue
		D3D12_COMMAND_QUEUE_DESC queueDesc = {};
		queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
		queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
		if (FAILED(device->device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&device->commandQueue)))) return false;

		return true;
	}

	D3D12_EXTERN void Orbital_Video_D3D12_Device_Dispose(Device* device)
	{
		if (device->commandQueue != NULL)
		{
			device->commandQueue->Release();
			device->commandQueue = NULL;
		}

		if (device != NULL)
		{
			device->device->Release();
			device->device = NULL;
		}

		if (device->adapter != NULL)
		{
			device->adapter->Release();
			device->adapter = NULL;
		}

		if (device->factory != NULL)
		{
			device->factory->Release();
			device->factory = NULL;
		}

		if (device->debugController != NULL)
		{
			device->debugController->Release();
			device->debugController = NULL;
		}

		free(device);
	}
}