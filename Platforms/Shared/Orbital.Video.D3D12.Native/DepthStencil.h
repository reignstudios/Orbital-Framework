#pragma once
#include "Device.h"

struct DepthStencil
{
	Device* device;
	DepthStencilMode mode;
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE resourceCPUHeapHandle;
	DXGI_FORMAT format;
	D3D12_RESOURCE_STATES resourceState;
};

void Orbital_Video_D3D12_DepthStencil_ChangeState(DepthStencil* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);