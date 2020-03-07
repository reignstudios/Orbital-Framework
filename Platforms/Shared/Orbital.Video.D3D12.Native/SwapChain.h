#pragma once
#include "Device.h"

struct SwapChain
{
	Device* device;
	UINT bufferCount, currentRenderTargetIndex;
	IDXGISwapChain3* swapChain;
	ID3D12Resource** resources;
	ID3D12DescriptorHeap* resourceHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE* resourceDescCPUHandles;
	DXGI_FORMAT format;
	D3D12_RESOURCE_STATES resourceState;

	// used to switch over to present
	ID3D12GraphicsCommandList5* internalCommandList;
	ID3D12Fence* internalFence;
	HANDLE internalFenceEvent;
	UINT64 internalFenceValue;
};

void Orbital_Video_D3D12_SwapChain_ChangeState(SwapChain* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);