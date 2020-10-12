#pragma once
#include "Device.h"

struct ConstantBufferNode
{
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE resourceHeapHandle;
	D3D12_RESOURCE_STATES resourceState;
	UINT8* updateDataPtr;
};

struct ConstantBuffer
{
	Device* device;
	ConstantBufferMode mode;
	ConstantBufferNode* nodes;
};

void Orbital_Video_D3D12_ConstantBuffer_ChangeState(ConstantBuffer* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);