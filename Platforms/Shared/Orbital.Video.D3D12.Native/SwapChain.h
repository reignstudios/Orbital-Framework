#pragma once
#include "Device.h"

struct SwapChain
{
	Device* device;
	UINT bufferCount, currentRenderTargetIndex;
	IDXGISwapChain3* swapChain;
	ID3D12Resource** resources;
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE* resourceDescCPUHandles;
	DXGI_FORMAT format;
	D3D12_RESOURCE_STATES resourceState;
};