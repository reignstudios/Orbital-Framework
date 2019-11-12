#pragma once
#include "Common.h"
#include "Instance.h"

struct Device
{
	#if defined(_DEBUG)
	ID3D12Debug* debugController;
	#endif

	//IDXGIFactory4* factory;
	Instance* instance;
	IDXGIAdapter* adapter;
	ID3D12Device* device;
	ID3D12CommandQueue* commandQueue;
	ID3D12CommandAllocator* commandAllocator;
	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;
};