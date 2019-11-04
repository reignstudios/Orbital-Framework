#pragma once
#include <d3d12.h>

struct Device
{
#if defined(_DEBUG)
	ID3D12Debug* debugController;
#endif

	ID3D12Device* device;

	bool Init();
	void Dispose();
};