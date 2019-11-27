#include "Device.h"
#include "CommandList.h"

extern "C"
{
	ORBITAL_EXPORT Device* Orbital_Video_D3D12_Device_Create(Instance* instance)
	{
		Device* handle = (Device*)calloc(1, sizeof(Device));
		handle->instance = instance;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Device_Init(Device* handle, int adapterIndex, int softwareRasterizer)
	{
		// get adapter
		if (softwareRasterizer)
		{
			if (FAILED(handle->instance->factory->EnumWarpAdapter(IID_PPV_ARGS(&handle->adapter)))) return 0;
		}
		else if (adapterIndex != -1)
		{
			IDXGIAdapter1* adapter1 = NULL;
			if (FAILED(handle->instance->factory->EnumAdapters1(adapterIndex, &adapter1))) return 0;
			handle->adapter = adapter1;
		}

		// create device
		if (FAILED(D3D12CreateDevice(handle->adapter, handle->instance->nativeMinFeatureLevel, IID_PPV_ARGS(&handle->device)))) return 0;
		handle->nodeCount = handle->device->GetNodeCount();

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
		handle->nativeFeatureLevel = featureLevelInfo.MaxSupportedFeatureLevel;

		// validate max isn't less than min
		if (handle->nativeFeatureLevel < handle->instance->nativeMinFeatureLevel) return 0;

		// get root signature version
		D3D12_FEATURE_DATA_ROOT_SIGNATURE rootSignature = {};
		if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_ROOT_SIGNATURE, &rootSignature, sizeof(D3D12_FEATURE_DATA_ROOT_SIGNATURE)))) return 0;
		handle->maxRootSignatureVersion = rootSignature.HighestVersion;

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
}