#pragma once
#include "Common.h"
#include "Device.h"

struct SwapChain
{
	Device* device;
	UINT bufferCount, currentRenderTargetIndex;
	IDXGISwapChain3* swapChain;
	ID3D12DescriptorHeap* renderTargetViewHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetDescHandles;
	ID3D12Resource** renderTargetViews;
};