#pragma once
#include "Instance.h"

struct Device
{
	#if defined(_DEBUG)
	ID3D12Debug* debugController;
	#endif

	D3D_FEATURE_LEVEL nativeFeatureLevel;
	Instance* instance;
	IDXGIAdapter* adapter;
	ID3D12Device* physicalDevice;
	ID3D12CommandQueue* commandQueue;
	ID3D12CommandAllocator* commandAllocator;
	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;
};