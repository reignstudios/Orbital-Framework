#pragma once
#include "Common.h"

struct SwapChain
{
	UINT bufferCount;
	IDXGISwapChain1* swapChain;
	ID3D12DescriptorHeap* renderTargetViewHeap;
	ID3D12Resource** renderTargetResources;
};