#pragma once
#include "Device.h"

struct TextureNode
{
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* shaderResourceHeap;
	ID3D12DescriptorHeap* renderTargetResourceHeap;
	ID3D12DescriptorHeap* randomAccessResourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE renderTargetResourceDescCPUHandle;
	D3D12_RESOURCE_STATES resourceState;
};

struct Texture
{
	Device* device;
	TextureMode mode;
	TextureNode* nodes;
	DXGI_FORMAT format;
	DXGI_SAMPLE_DESC msaaSampleDesc;
};

void Orbital_Video_D3D12_Texture_ChangeState(Texture* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);