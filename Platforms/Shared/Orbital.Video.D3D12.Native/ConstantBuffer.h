#pragma once
#include "Device.h"

struct ConstantBuffer
{
	Device* device;
	ConstantBufferMode mode;
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE resourceHeapHandle;
	D3D12_RESOURCE_STATES resourceState;
	UINT8* updateDataPtr;
};

void Orbital_Video_D3D12_ConstantBuffer_ChangeState(ConstantBuffer* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);