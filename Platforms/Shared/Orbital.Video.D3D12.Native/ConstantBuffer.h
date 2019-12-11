#pragma once
#include "Device.h"

struct ConstantBuffer
{
	Device* device;
	ID3D12Resource* resource;
	ID3D12DescriptorHeap* heap;
};