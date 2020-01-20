#pragma once
#include "Device.h"

struct Texture
{
	Device* device;
	TextureMode mode;
	ID3D12Resource* texture;
	ID3D12DescriptorHeap* textureHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE textureHeapHandle;
	DXGI_FORMAT format;
};