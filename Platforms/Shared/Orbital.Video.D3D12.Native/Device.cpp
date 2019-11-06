#include "Device.h"

extern "C"
{
	ORBITAL_EXPORT Device* Orbital_Video_D3D12_Device_Create()
	{
		return (Device*)calloc(1, sizeof(Device));
	}

	ORBITAL_EXPORT bool Orbital_Video_D3D12_Device_Init(Device* handle, int adapterIndex, FeatureLevel minimumFeatureLevel, bool softwareRasterizer)
	{
		// get native feature level
		D3D_FEATURE_LEVEL nativeMinFeatureLevel;
		switch (minimumFeatureLevel)
		{
			case FeatureLevel::Level_11_0: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_0; break;
			case FeatureLevel::Level_11_1: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_1; break;
			case FeatureLevel::Level_12_0: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_0; break;
			case FeatureLevel::Level_12_1: nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_1; break;
			default: return false;
		}

		// enable debugging
		UINT factoryFlags = 0;
		#if defined(_DEBUG)
		if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&handle->debugController))))
		{
			handle->debugController->EnableDebugLayer();
			factoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
		}
		#endif

		if (FAILED(CreateDXGIFactory2(factoryFlags, IID_PPV_ARGS(&handle->factory)))) return false;

		// get adapter
		if (softwareRasterizer)
		{
			if (FAILED(handle->factory->EnumWarpAdapter(IID_PPV_ARGS(&handle->adapter)))) return false;
		}
		else
		{
			IDXGIAdapter1* adapter1 = NULL;
			if (adapterIndex != -1)
			{
				for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != handle->factory->EnumAdapters1(adapterIndex, &adapter1); ++adapterIndex)
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
						handle->adapter = adapter1;
						break;
					}

					adapter1->Release();
				}

				if (handle->adapter == NULL) return false;
			}
		}

		// create device
		if (FAILED(D3D12CreateDevice(handle->adapter, nativeMinFeatureLevel, IID_PPV_ARGS(&handle->device)))) return false;

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
		if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_FEATURE_LEVELS, &featureLevelInfo, sizeof(D3D12_FEATURE_DATA_FEATURE_LEVELS)))) return false;

		// validate max isn't less than min
		if (featureLevelInfo.MaxSupportedFeatureLevel < nativeMinFeatureLevel) return false;

		// create command queue
		D3D12_COMMAND_QUEUE_DESC queueDesc = {};
		queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
		queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
		if (FAILED(handle->device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&handle->commandQueue)))) return false;

		return true;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_Dispose(Device* handle)
	{
		if (handle->commandQueue != NULL)
		{
			handle->commandQueue->Release();
			handle->commandQueue = NULL;
		}

		if (handle != NULL)
		{
			handle->device->Release();
			handle->device = NULL;
		}

		if (handle->adapter != NULL)
		{
			handle->adapter->Release();
			handle->adapter = NULL;
		}

		if (handle->factory != NULL)
		{
			handle->factory->Release();
			handle->factory = NULL;
		}

		if (handle->debugController != NULL)
		{
			handle->debugController->Release();
			handle->debugController = NULL;
		}

		free(handle);
	}
}