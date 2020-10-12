#pragma once
#include "VertexBuffer.h"

struct VertexBufferStreamerNode
{
	D3D12_VERTEX_BUFFER_VIEW* vertexBufferViews;
};

struct VertexBufferStreamer
{
	Device* device;
	UINT vertexBufferCount;
	VertexBuffer** vertexBuffers;
	VertexBufferStreamerNode* nodes;

	UINT elementCount;
	D3D12_INPUT_ELEMENT_DESC* elements;
};