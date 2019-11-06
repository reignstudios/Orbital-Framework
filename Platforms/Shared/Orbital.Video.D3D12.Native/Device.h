#pragma once
#include <d3d12.h>
#include <dxgi1_6.h>

enum FeatureLevel
{
	Level_9_1,
	Level_9_2,
	Level_9_3,
	Level_10_0,
	Level_10_1,
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
};