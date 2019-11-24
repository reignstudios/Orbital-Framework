#include "Instance.h"

extern "C"
{
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

	ORBITAL_EXPORT Instance* Orbital_Video_D3D12_Instance_Create()
	{
		return (Instance*)calloc(1, sizeof(Instance));
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Instance_Init(Instance* handle, FeatureLevel minimumFeatureLevel)
	{
		// get native feature level
		if (!FeatureLevelToNative(minimumFeatureLevel, &handle->nativeMinFeatureLevel)) return 0;

		// enable debugging
		UINT factoryFlags = 0;
		#if defined(_DEBUG)
		if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&handle->debugController))))
		{
			handle->debugController->EnableDebugLayer();
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

		if (handle->debugController != NULL)
		{
			handle->debugController->Release();
			handle->debugController = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Instance_QuerySupportedAdapters(Instance* handle, int allowSoftwareAdapters, WCHAR** adapterNames, UINT adapterNameMaxLength, UINT* adapterIndices, UINT* adapterCount)
	{
		IDXGIAdapter1* adapter1 = NULL;
		UINT maxAdapterCount = *adapterCount;
		*adapterCount = 0;
		for (UINT i = 0; DXGI_ERROR_NOT_FOUND != handle->factory->EnumAdapters1(i, &adapter1); ++i)
		{
			// early out if we reached max adapter count
			if (i >= maxAdapterCount)
			{
				adapter1->Release();
				handle->factory->Release();
				return 1;
			}

			// get adapter desc
			DXGI_ADAPTER_DESC1 desc;
			if (FAILED(adapter1->GetDesc1(&desc)))
			{
				adapter1->Release();
				handle->factory->Release();
				return 0;
			}

			// check if software adapter
			if (!allowSoftwareAdapters && desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
			{
				adapter1->Release();
				continue;
			}

			// make sure adapter can be used
			if (FAILED(D3D12CreateDevice(adapter1, handle->nativeMinFeatureLevel, _uuidof(ID3D12Device), nullptr)))
			{
				adapter1->Release();
				continue;
			}

			// add name and increase count
			UINT maxLength = min(sizeof(WCHAR) * adapterNameMaxLength, sizeof(desc.Description));
			memcpy(adapterNames[(*adapterCount)], desc.Description, maxLength);
			adapterIndices[(*adapterCount)] = i;
			++(*adapterCount);

			// finish
			adapter1->Release();
		}

		return 1;
	}
}