#pragma once
#include "Device.h"

struct IndexBufferNode
{
	ID3D12Resource* resource;
	D3D12_INDEX_BUFFER_VIEW resourceView;
	D3D12_RESOURCE_STATES resourceState;
};

struct IndexBuffer
{
	Device* device;
	IndexBufferMode mode;
	IndexBufferNode* nodes;
};

void Orbital_Video_D3D12_IndexBuffer_ChangeState(IndexBuffer* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);