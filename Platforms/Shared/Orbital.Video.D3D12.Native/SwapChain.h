#pragma once
#include "Device.h"

struct SwapChainNode
{
	ID3D12DescriptorHeap* resourceHeap;
};

struct SwapChain
{
	Device* device;
	SwapChainNode* nodes;
	UINT bufferCount, nodeCount, currentRenderTargetIndex, currentNodeIndex;
	IDXGISwapChain* swapChain;
	IDXGISwapChain1* swapChain1;
	IDXGISwapChain3* swapChain3;
	ID3D12Resource** resources;
	D3D12_CPU_DESCRIPTOR_HANDLE* resourceDescCPUHandles;
	D3D12_RESOURCE_STATES* resourceStates;
	DXGI_FORMAT format;
	SwapChainType type;
	UINT vSync;
	SwapChainVSyncMode vSyncMode;
	bool fullscreen;

	// used to switch over to present
	ID3D12CommandAllocator* internalCommandAllocator;
	ID3D12GraphicsCommandList* internalCommandList;
	ID3D12Fence* internalFence;
	HANDLE internalFenceEvent;
	UINT64 internalFenceValue;
};

void Orbital_Video_D3D12_SwapChain_ChangeState(SwapChain* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);