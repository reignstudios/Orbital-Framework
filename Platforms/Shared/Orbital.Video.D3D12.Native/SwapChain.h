#pragma once
#include "Device.h"

struct SwapChainNode
{
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE resourceDescCPUHandle;
	D3D12_RESOURCE_STATES resourceState;
};

struct SwapChain
{
	Device* device;
	SwapChainNode* nodes;
	UINT bufferCount, nodeCount, currentRenderTargetIndex, currentNodeIndex;
	IDXGISwapChain3* swapChain;
	ID3D12Resource** resources;
	DXGI_FORMAT format;
	SwapChainType type;

	// used to switch over to present
	ID3D12CommandAllocator* internalCommandAllocator;
	ID3D12GraphicsCommandList* internalCommandList;
	ID3D12Fence* internalFence;
	HANDLE internalFenceEvent;
	UINT64 internalFenceValue;
};

void Orbital_Video_D3D12_SwapChain_ChangeState(SwapChain* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);