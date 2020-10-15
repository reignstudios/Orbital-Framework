#include "Device.h"
#include "CommandList.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT Device* Orbital_Video_D3D12_Device_Create(Instance* instance, DeviceType type)
	{
		Device* handle = (Device*)calloc(1, sizeof(Device));
		handle->instance = instance;
		handle->type = type;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Device_Init(Device* handle, int adapterIndex, int softwareRasterizer, int allowMultiGPU, int* nodeCount)
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

		// get mGPU node info
		handle->fullNodeCount = handle->device->GetNodeCount();
		if (allowMultiGPU)
		{
			handle->nodeCount = handle->fullNodeCount;
			handle->fullNodeMask = (1 << handle->nodeCount) - 1;
		}
		else
		{
			handle->nodeCount = 1;
			handle->fullNodeMask = 1;
		}
		*nodeCount = handle->nodeCount;

		// get max feature level
		D3D_FEATURE_LEVEL supportedFeatureLevels[4] =
		{
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_11_1,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_0,
			D3D_FEATURE_LEVEL::D3D_FEATURE_LEVEL_12_1
		};
		D3D12_FEATURE_DATA_FEATURE_LEVELS featureLevelInfo = {};
		featureLevelInfo.NumFeatureLevels = 4;
		featureLevelInfo.pFeatureLevelsRequested = supportedFeatureLevels;
		if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_FEATURE_LEVELS, &featureLevelInfo, sizeof(D3D12_FEATURE_DATA_FEATURE_LEVELS)))) return 0;
		handle->nativeFeatureLevel = featureLevelInfo.MaxSupportedFeatureLevel;

		// validate max isn't less than min
		if (handle->nativeFeatureLevel < handle->instance->nativeMinFeatureLevel) return 0;

		// get root signature version
		D3D12_FEATURE_DATA_ROOT_SIGNATURE rootSignature = {};
		rootSignature.HighestVersion = D3D_ROOT_SIGNATURE_VERSION_1_1;
		if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_ROOT_SIGNATURE, &rootSignature, sizeof(D3D12_FEATURE_DATA_ROOT_SIGNATURE))))
		{
			rootSignature.HighestVersion = D3D_ROOT_SIGNATURE_VERSION_1_0;
			if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_ROOT_SIGNATURE, &rootSignature, sizeof(D3D12_FEATURE_DATA_ROOT_SIGNATURE)))) return 0;
		}
		handle->maxRootSignatureVersion = rootSignature.HighestVersion;

		// create nodes
		handle->nodes = (DeviceNode*)calloc(handle->nodeCount, sizeof(DeviceNode));
		for (UINT n = 0; n != handle->nodeCount; ++n)
		{
			UINT nodeMask = 1 << n;
			handle->nodes[n].mask = nodeMask;

			// create command queues
			D3D12_COMMAND_QUEUE_DESC queueDesc = {};
			queueDesc.NodeMask = nodeMask;
			queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
			queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
			if (FAILED(handle->device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&handle->nodes[n].commandQueue)))) return 0;

			queueDesc.Type = D3D12_COMMAND_LIST_TYPE_COMPUTE;
			if (FAILED(handle->device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&handle->nodes[n].commandQueue_Compute)))) return 0;

			// create command allocators
			if (FAILED(handle->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&handle->nodes[n].internalCommandAllocator)))) return 0;
			if (FAILED(handle->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_COMPUTE, IID_PPV_ARGS(&handle->nodes[n].internalCommandAllocator_Compute)))) return 0;

			// create helpers for synchronous buffer operations
			if (FAILED(handle->device->CreateCommandList(nodeMask, D3D12_COMMAND_LIST_TYPE_DIRECT, handle->nodes[n].internalCommandAllocator, nullptr, IID_PPV_ARGS(&handle->nodes[n].internalCommandList)))) return 0;
			if (FAILED(handle->nodes[n].internalCommandList->Close())) return 0;// make sure this is closed as it defaults to open for writing

			if (FAILED(handle->device->CreateCommandList(nodeMask, D3D12_COMMAND_LIST_TYPE_COMPUTE, handle->nodes[n].internalCommandAllocator_Compute, nullptr, IID_PPV_ARGS(&handle->nodes[n].internalCommandList_Compute)))) return 0;
			if (FAILED(handle->nodes[n].internalCommandList_Compute->Close())) return 0;// make sure this is closed as it defaults to open for writing

			// create main fence
			if (FAILED(handle->device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&handle->nodes[n].fence)))) return 0;
			handle->nodes[n].fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			if (handle->nodes[n].fenceEvent == NULL) return 0;

			// create internal fence
			if (FAILED(handle->device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&handle->nodes[n].internalFence)))) return 0;
			handle->nodes[n].internalFenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			if (handle->nodes[n].internalFenceEvent == NULL) return 0;

			// make sure fence values start at 1 so they don't match 'GetCompletedValue' when its first called
			handle->nodes[n].fenceValue = 1;
			handle->nodes[n].internalFenceValue = 1;
			handle->nodes[n].internalMutex = new std::mutex();
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_Dispose(Device* handle)
	{
		if (handle->nodes != NULL)
		{
			for (UINT n = 0; n != handle->nodeCount; ++n)
			{
				// dispose helpers
				if (handle->nodes[n].internalFenceEvent != NULL)
				{
					CloseHandle(handle->nodes[n].internalFenceEvent);
					handle->nodes[n].internalFenceEvent = NULL;
				}

				if (handle->nodes[n].internalFence != NULL)
				{
					handle->nodes[n].internalFence->Release();
					handle->nodes[n].internalFence = NULL;
				}

				if (handle->nodes[n].internalCommandList != NULL)
				{
					handle->nodes[n].internalCommandList->Release();
					handle->nodes[n].internalCommandList = NULL;
				}

				if (handle->nodes[n].internalCommandList_Compute != NULL)
				{
					handle->nodes[n].internalCommandList_Compute->Release();
					handle->nodes[n].internalCommandList_Compute = NULL;
				}

				// dispose normal
				if (handle->nodes[n].fenceEvent != NULL)
				{
					CloseHandle(handle->nodes[n].fenceEvent);
					handle->nodes[n].fenceEvent = NULL;
				}

				if (handle->nodes[n].fence != NULL)
				{
					handle->nodes[n].fence->Release();
					handle->nodes[n].fence = NULL;
				}

				if (handle->nodes[n].internalCommandAllocator != NULL)
				{
					handle->nodes[n].internalCommandAllocator->Release();
					handle->nodes[n].internalCommandAllocator = NULL;
				}

				if (handle->nodes[n].internalCommandAllocator_Compute != NULL)
				{
					handle->nodes[n].internalCommandAllocator_Compute->Release();
					handle->nodes[n].internalCommandAllocator_Compute = NULL;
				}

				if (handle->nodes[n].commandQueue != NULL)
				{
					handle->nodes[n].commandQueue->Release();
					handle->nodes[n].commandQueue = NULL;
				}

				if (handle->nodes[n].commandQueue_Compute != NULL)
				{
					handle->nodes[n].commandQueue_Compute->Release();
					handle->nodes[n].commandQueue_Compute = NULL;
				}

				if (handle->nodes[n].internalMutex != NULL)
				{
					delete handle->nodes[n].internalMutex;
					handle->nodes[n].internalMutex = NULL;
				}
			}

			free(handle->nodes);
			handle->nodes = NULL;
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

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_BeginFrame(Device* handle)
	{
		/*for (UINT n = 0; n != handle->nodeCount; ++n)
		{
			handle->nodes[n].internalCommandAllocator->Reset();
			handle->nodes[n].internalCommandAllocator_Compute->Reset();
		}*/
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Device_EndFrame(Device* handle)
	{
		for (UINT n = 0; n != handle->nodeCount; ++n)
		{
			WaitForFence(handle, n, handle->nodes[n].fence, handle->nodes[n].fenceEvent, handle->nodes[n].fenceValue);
		}
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Device_GetMaxMSAALevel(Device* handle, TextureFormat format, MSAALevel* msaaLevel)
	{
		DXGI_FORMAT nativeFormat;
		if (!GetNative_TextureFormat(format, &nativeFormat, true)) return 0;
		*msaaLevel = MSAALevel::MSAALevel_Disabled;
		UINT lvl = 2;
		for (UINT i = 0; i != 4; ++i)
		{
			D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS featureData = {};
			featureData.SampleCount = lvl;
			lvl *= 2;
			featureData.NumQualityLevels = 0;
			featureData.Format = nativeFormat;
			if (FAILED(handle->device->CheckFeatureSupport(D3D12_FEATURE_MULTISAMPLE_QUALITY_LEVELS, &featureData, sizeof(D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS)))) return 0;
			if (featureData.NumQualityLevels == 0) return 1;// max reached
			*msaaLevel = (MSAALevel)featureData.SampleCount;
		}
		return 1;
	}
}

void WaitForFence_CommandQueue(ID3D12CommandQueue* commandQueue, ID3D12Fence* fence, HANDLE fenceEvent, UINT64& fenceValue)
{
	// increment for next frame
	++fenceValue;
	if (fenceValue == UINT64_MAX) fenceValue = 0;// UINT64_MAX is reserved

	// set current fence value
	if (FAILED(commandQueue->Signal(fence, fenceValue))) return;

	// wait for frame to finish
	if (fence->GetCompletedValue() != fenceValue)
	{
		if (FAILED(fence->SetEventOnCompletion(fenceValue, fenceEvent))) return;
		WaitForSingleObject(fenceEvent, INFINITE);
	}
}

void WaitForFence(Device* handle, UINT nodeIndex, ID3D12Fence* fence, HANDLE fenceEvent, UINT64& fenceValue)
{
	WaitForFence_CommandQueue(handle->nodes[nodeIndex].commandQueue, fence, fenceEvent, fenceValue);
	WaitForFence_CommandQueue(handle->nodes[nodeIndex].commandQueue_Compute, fence, fenceEvent, fenceValue);
}