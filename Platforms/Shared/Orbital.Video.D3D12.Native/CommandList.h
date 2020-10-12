#pragma once
#include "Device.h"

struct CommandListNode
{
	ID3D12CommandAllocator *commandAllocator, *commandAllocator_Direct;
	ID3D12GraphicsCommandList *commandList;
	ID3D12GraphicsCommandList4 *commandList4;
	ID3D12GraphicsCommandList *commandList_Direct;// can always be used for direct commands even if 'commandList' is compute or rayTrace
	ID3D12Fence* fence;
	HANDLE fenceEvent;
	UINT64 fenceValue;

	ID3D12CommandQueue* commandQueueRef, *commandQueueRef_Direct;
};

struct CommandList
{
	Device* device;
	CommandListNode* nodes;
	CommandListNode* activeNode;
	UINT activeNodeIndex;
};