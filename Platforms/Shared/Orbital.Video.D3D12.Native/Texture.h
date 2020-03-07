#pragma once
#include "Device.h"

struct Texture
{
	Device* device;
	TextureMode mode;
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* shaderResourceHeap;
	ID3D12DescriptorHeap* renderTargetResourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE renderTargetResourceDescCPUHandle;
	DXGI_FORMAT format;
	D3D12_RESOURCE_STATES resourceState;
	DXGI_SAMPLE_DESC msaaSampleDesc;
};

void Orbital_Video_D3D12_Texture_ChangeState(Texture* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);