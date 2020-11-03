#include "Instance.h"

bool FeatureLevelToNative(FeatureLevel featureLevel, D3D_FEATURE_LEVEL* nativeMinFeatureLevel)
{
	switch (featureLevel)
	{
		case FeatureLevel::Level_11_0: *nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_0; break;
		case FeatureLevel::Level_11_1: *nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_1; break;
		case FeatureLevel::Level_12_0: *nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_0; break;
		case FeatureLevel::Level_12_1: *nativeMinFeatureLevel = D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_1; break;
		default: return false;
	}
	return true;
}

extern "C"
{
	ORBITAL_EXPORT Instance* Orbital_Video_D3D12_Instance_Create()
	{
		return (Instance*)calloc(1, sizeof(Instance));
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Instance_Init(Instance* handle, FeatureLevel minimumFeatureLevel, int extraDebugging)
	{
		// get native feature level
		if (!FeatureLevelToNative(minimumFeatureLevel, &handle->nativeMinFeatureLevel)) return 0;

		// enable debugging
		UINT factoryFlags = 0;
		#if defined(_DEBUG)
		if (IsDebuggerPresent())// only attach if debugger is present. Otherwise some drivers can have issues
		if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&handle->debugController))))
		{
			if (extraDebugging) handle->debugController->QueryInterface(IID_PPV_ARGS(&handle->debugController3));
			if (handle->debugController3 != nullptr)
			{
				handle->debugController3->SetEnableGPUBasedValidation(true);
				handle->debugController3->SetGPUBasedValidationFlags(D3D12_GPU_BASED_VALIDATION_FLAGS_DISABLE_STATE_TRACKING);
				handle->debugController3->SetEnableSynchronizedCommandQueueValidation(true);
				handle->debugController3->EnableDebugLayer();
			}
			else
			{
				handle->debugController->EnableDebugLayer();
			}
			factoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
		}
		#endif

		if (FAILED(CreateDXGIFactory2(factoryFlags, IID_PPV_ARGS(&handle->factory)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Instance_Dispose(Instance* handle)
	{
		if (handle->factory != NULL)
		{
			handle->factory->Release();
			handle->factory = NULL;
		}

		#if defined(_DEBUG)
		if (handle->debugController3 != NULL)
		{
			handle->debugController3->Release();
			handle->debugController3 = NULL;
		}

		if (handle->debugController != NULL)
		{
			handle->debugController->Release();
			handle->debugController = NULL;
		}
		#endif

		free(handle);
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Instance_QuerySupportedAdapters(Instance* handle, int allowSoftwareAdapters, AdapterInfo* adapters, int* adapterCount, int adapterNameMaxLength)
	{
		IDXGIAdapter1* adapterD3D12 = NULL;
		UINT maxAdapterCount = *adapterCount;
		*adapterCount = 0;
		for (UINT i = 0; DXGI_ERROR_NOT_FOUND != handle->factory->EnumAdapters1(i, &adapterD3D12); ++i)
		{
			auto adapter = &adapters[(*adapterCount)];

			// early out if we reached max adapter count
			if (i >= maxAdapterCount)
			{
				adapterD3D12->Release();
				handle->factory->Release();
				return 1;
			}

			// get adapter desc
			DXGI_ADAPTER_DESC1 desc;
			if (FAILED(adapterD3D12->GetDesc1(&desc)))
			{
				adapterD3D12->Release();
				handle->factory->Release();
				return 0;
			}

			// check if software adapter
			if (!allowSoftwareAdapters && desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
			{
				adapterD3D12->Release();
				continue;
			}

			// set is primary if index is 0
			if (i == i) adapter->isPrimary = 1;
			adapter->vendorID = desc.VendorId;

			// make sure adapter can be used
			ID3D12Device* device = nullptr;
			if (FAILED(D3D12CreateDevice(adapterD3D12, handle->nativeMinFeatureLevel, IID_PPV_ARGS(&device))))
			{
				adapterD3D12->Release();
				continue;
			}

			// get adapter node count
			adapter->nodeCount = device->GetNodeCount();
			device->Release();

			// add name and increase count
			UINT maxLength = min(sizeof(WCHAR) * adapterNameMaxLength, sizeof(desc.Description));
			memcpy(adapter->name, desc.Description, maxLength);
			adapter->index = i;
			++(*adapterCount);

			// get GPU memory
			adapter->dedicatedGPUMemory = desc.DedicatedVideoMemory;
			adapter->deticatedSystemMemory = desc.DedicatedSystemMemory;
			adapter->sharedSystemMemory = desc.SharedSystemMemory;

			// finish
			adapterD3D12->Release();
		}

		return 1;
	}
}