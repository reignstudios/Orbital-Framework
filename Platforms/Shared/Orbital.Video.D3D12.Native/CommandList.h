#pragma once
#include "Device.h"

struct CommandList
{
	Device* device;
	ID3D12GraphicsCommandList5 *commandList, *commandList_Direct;
	ID3D12CommandAllocator *commandAllocatorRef, *commandAllocatorRef_Direct;
	ID3D12CommandQueue *commandQueueRef, *commandQueueRef_Direct;

	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;
};