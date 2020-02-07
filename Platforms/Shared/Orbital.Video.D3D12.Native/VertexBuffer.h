#pragma once
#include "Device.h"

struct VertexBuffer
{
	Device* device;
	VertexBufferMode mode;
	ID3D12Resource* vertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW vertexBufferView;
	D3D12_RESOURCE_STATES resourceState;
};

void Orbital_Video_D3D12_VertexBuffer_ChangeState(VertexBuffer* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);