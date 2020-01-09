#pragma once
#include "Instance.h"

struct Device
{
	D3D_FEATURE_LEVEL nativeFeatureLevel;
	D3D_ROOT_SIGNATURE_VERSION maxRootSignatureVersion;

	Instance* instance;
	IDXGIAdapter* adapter;
	ID3D12Device* device;
	UINT nodeCount;
	ID3D12CommandQueue* commandQueue;
	ID3D12CommandAllocator* commandAllocator;
	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;

	// used for special synchronous buffer operations
	ID3D12GraphicsCommandList5* internalCommandList;
	ID3D12Fence* internalFence;
	HANDLE internalFenceEvent;
	UINT64 internalFenceValue;
};

void WaitForFence(Device* handle, ID3D12Fence* fence, HANDLE fenceEvent, UINT64& fenceValue);