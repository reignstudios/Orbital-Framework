#pragma once
#include "Device.h"

struct SwapChain
{
	Device* device;
	UINT bufferCount, currentRenderTargetIndex;
	IDXGISwapChain3* swapChain;
	ID3D12DescriptorHeap* renderTargetViewHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetDescCPUHandles;
	ID3D12Resource** renderTargetViews;
	DXGI_FORMAT renderTargetFormat;
};