#pragma once
#include "Common.h"

enum FeatureLevel
{
	Level_11_0,
	Level_11_1,
	Level_12_0,
	Level_12_1
};

struct Device
{
	#if defined(_DEBUG)
	ID3D12Debug* debugController;
	#endif

	IDXGIFactory4* factory;
	IDXGIAdapter* adapter;
	ID3D12Device* device;
	ID3D12CommandQueue* commandQueue;
	ID3D12CommandAllocator* commandAllocator;
	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;
};