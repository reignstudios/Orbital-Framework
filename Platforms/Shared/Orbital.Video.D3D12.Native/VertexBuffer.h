#pragma once
#include "Device.h"

struct VertexBufferNode
{
	ID3D12Resource* resource;
	D3D12_VERTEX_BUFFER_VIEW resourceView;
	D3D12_RESOURCE_STATES resourceState;
};

struct VertexBuffer
{
	Device* device;
	VertexBufferMode mode;
	VertexBufferNode* nodes;
};

void Orbital_Video_D3D12_VertexBuffer_ChangeState(VertexBuffer* handle, UINT nodeIndex, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList* commandList);