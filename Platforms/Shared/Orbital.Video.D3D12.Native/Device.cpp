#include "Device.h"
#include "CommandBuffer.h"

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

	ORBITAL_EXPORT int Orbital_Video_D3D12_Device_QuerySupportedAdapters(FeatureLevel minimumFeatureLevel, int allowSoftwareAdapters, WCHAR** adapterNames, UINT* adapterNameCount, UINT adapterNameMaxLength)
	{
		D3D_FEATURE_LEVEL nativeMinFeatureLevel;
		if (!FeatureLevelToNative(minimumFeatureLevel, &nativeMinFeatureLevel)) return 0;

		IDXGIFactory4* factory = NULL;
		if (FAILED(CreateDXGIFactory2(0, IID_PPV_ARGS(&factory)))) return 0;

		IDXGIAdapter1* adapter1 = NULL;
		UINT maxAdapterCount = *adapterNameCount;
		*adapterNameCount = 0;
		for (UINT i = 0; DXGI_ERROR_NOT_FOUND != factory->EnumAdapters1(i, &adapter1); ++i)
		{
			// early out if we reached max adapter count
			if (i >= maxAdapterCount)
			{
				adapter1->Release();
				factory->Release();
				return 1;
			}

			// get adapter desc
			DXGI_ADAPTER_DESC1 desc;
			if (FAILED(adapter1->GetDesc1(&desc)))
			{
				adapter1->Release();
				factory->Release();
				return 0;
			}

			// check if software adapter
			if (!allowSoftwareAdapters && desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
			{
				adapter1->Release();
				continue;
			}

			// make sure adapter can be used
			if (FAILED(D3D12CreateDevice(adapter1, nativeMinFeatureLevel, _uuidof(ID3D12Device), nullptr)))
			{
				adapter1->Release();
				continue;
			}

			// add name and increase count
			UINT maxLength = min(sizeof(WCHAR) * adapterNameMaxLength, sizeof(desc.Description));
			memcpy(adapterNames[i], desc.Description, maxLength);
			++(*adapterNameCount);

			// finish
			adapter1->Release();
		}

		factory->Release();
		return 1;
	}

	ORBITAL_EXPORT Device* Orbital_Video_D3D12_Device_Create()
	{
		return (Device*)calloc(1, sizeof(Device));
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Device_Init(Device* handle, int adapterIndex, FeatureLevel minimumFeatureLevel, int softwareRasterizer)
	{
		// get native feature level
		D3D_FEATURE_LEVEL nativeMinFeatureLevel;
		if (!FeatureLevelToNative(minimumFeatureLevel, &nativeMinFeatureLevel)) return 0;

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

		// get adapter
		if (softwareRasterizer)
		{
			if (FAILED(handle->factory->EnumWarpAdapter(IID_PPV_ARGS(&handle->adapter)))) return 0;
		}
		else if (adapterIndex != -1)
		{
			IDXGIAdapter1* adapter1 = NULL;
			if (FAILED(handle->factory->EnumAdapters1(adapterIndex, &adapter1))) return 0;
			handle->adapter = adapter1;
		}

		// create device
		if (FAILED(D3D12CreateDevice(handle->adapter, nativeMinFeatureLevel, IID_PPV_ARGS(&handle->device)))) return 0;

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
		if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_FEATURE_LEVELS, &featureLevelInfo, sizeof(D3D12_FEATURE_DATA_FEATURE_LEVELS)))) return 0;

		// validate max isn't less than min
		if (featureLevelInfo.MaxSupportedFeatureLevel < nativeMinFeatureLevel) return 0;

		// create command queue
		D3D12_COMMAND_QUEUE_DESC queueDesc = {};
		queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
		queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
		if (FAILED(handle->device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&handle->commandQueue)))) return 0;

		// create command allocator
		if (FAILED(handle->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&handle->commandAllocator)))) return 0;

		// create fence
		if (FAILED(handle->device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&handle->fence)))) return 0;
		handle->fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
		if (handle->fenceEvent == NULL) return 0;
		handle->fenceValue = 1;

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_Dispose(Device* handle)
	{
		if (handle->fenceEvent != NULL)
		{
			CloseHandle(handle->fenceEvent);
			handle->fenceEvent = NULL;
		}

		if (handle->fence != NULL)
		{
			handle->fence->Release();
			handle->fence = NULL;
		}

		if (handle->commandAllocator != NULL)
		{
			handle->commandAllocator->Release();
			handle->commandAllocator = NULL;
		}

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

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_BeginFrame(Device* handle)
	{
		handle->commandAllocator->Reset();
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_EndFrame(Device* handle)
	{
		// signal and increment the fence value.
		const UINT64 fence = handle->fenceValue;
		if (FAILED(handle->commandQueue->Signal(handle->fence, fence))) return;
		++handle->fenceValue;

		// wait until the previous frame is finished.
		if (handle->fence->GetCompletedValue() < fence)
		{
			if (FAILED(handle->fence->SetEventOnCompletion(fence, handle->fenceEvent))) return;
			WaitForSingleObject(handle->fenceEvent, INFINITE);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_ExecuteCommandBuffer(Device* handle, CommandBuffer* commandBuffer)
	{
		ID3D12CommandList* commandLists[1] = { commandBuffer->commandList };
		handle->commandQueue->ExecuteCommandLists(1, commandLists);
	}
}