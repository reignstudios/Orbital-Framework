#pragma once
#include "Device.h"

struct VertexBuffer
{
	Device* device;
	VertexBufferMode mode;
	ID3D12Resource* vertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW vertexBufferView;
	UINT elementCount;
	D3D12_INPUT_ELEMENT_DESC* elements;
};