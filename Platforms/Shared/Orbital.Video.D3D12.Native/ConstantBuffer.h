#pragma once
#include "Device.h"

struct ConstantBuffer
{
	Device* device;
	ConstantBufferMode mode;
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE resourceHeapHandle;
};