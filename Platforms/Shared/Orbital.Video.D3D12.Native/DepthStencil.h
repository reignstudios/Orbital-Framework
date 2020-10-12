#pragma once
#include "Device.h"

struct DepthStencilNode
{
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* shaderResourceHeap;
	ID3D12DescriptorHeap* depthStencilResourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE depthStencilResourceCPUHeapHandle;
	D3D12_RESOURCE_STATES resourceState;
};

struct DepthStencil
{
	Device* device;
	DepthStencilMode mode;
	DepthStencilNode* nodes;
	DXGI_FORMAT format;
};

void Orbital_Video_D3D12_DepthStencil_ChangeState(DepthStencil* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);