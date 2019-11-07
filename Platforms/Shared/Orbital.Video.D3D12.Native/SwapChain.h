#pragma once
#include "Common.h"

struct SwapChain
{
	UINT bufferCount, currentRenderTargetIndex;
	IDXGISwapChain3* swapChain;
	ID3D12DescriptorHeap* renderTargetViewHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetDescHandles;
	ID3D12Resource** renderTargetViews;
};