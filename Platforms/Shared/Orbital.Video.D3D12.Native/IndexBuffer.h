#pragma once
#include "Device.h"

struct IndexBuffer
{
	Device* device;
	IndexBufferMode mode;
	ID3D12Resource* indexBuffer;
    D3D12_INDEX_BUFFER_VIEW indexBufferView;
	D3D12_RESOURCE_STATES resourceState;
};

void Orbital_Video_D3D12_IndexBuffer_ChangeState(IndexBuffer* handle, D3D12_RESOURCE_STATES state, ID3D12GraphicsCommandList5* commandList);