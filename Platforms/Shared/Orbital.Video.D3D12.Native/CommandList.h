#pragma once
#include "Device.h"

struct CommandList
{
	Device* device;
	ID3D12GraphicsCommandList5* commandList;

	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;
};