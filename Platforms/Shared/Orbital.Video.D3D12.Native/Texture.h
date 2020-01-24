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
	D3D12_RESOURCE_STATES resourceState;
};

void Orbital_Video_D3D12_Texture_ChangeState(Texture* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);