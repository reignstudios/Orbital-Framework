#pragma once
#include "Instance.h"
#include <mutex>

struct DeviceNode
{
	UINT mask;

	ID3D12CommandQueue *commandQueue, *commandQueue_Compute;
	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;

	// used for special synchronous buffer operations
	ID3D12CommandAllocator *internalCommandAllocator, *internalCommandAllocator_Compute;
	ID3D12GraphicsCommandList *internalCommandList, *internalCommandList_Compute;
	ID3D12Fence* internalFence;
	HANDLE internalFenceEvent;
	UINT64 internalFenceValue;
	std::mutex* internalMutex;
};

struct Device
{
	D3D_FEATURE_LEVEL nativeFeatureLevel;
	D3D_ROOT_SIGNATURE_VERSION maxRootSignatureVersion;

	Instance* instance;
	DeviceType type;
	IDXGIAdapter* adapter;
	ID3D12Device* device;
	UINT nodeCount, fullNodeCount, fullNodeMask;
	DeviceNode* nodes;
};

void WaitForFence_CommandQueue(ID3D12CommandQueue* commandQueue, ID3D12Fence* fence, HANDLE fenceEvent, UINT64& fenceValue);
void WaitForFence(Device* handle, UINT nodeIndex, ID3D12Fence* fence, HANDLE fenceEvent, UINT64& fenceValue);