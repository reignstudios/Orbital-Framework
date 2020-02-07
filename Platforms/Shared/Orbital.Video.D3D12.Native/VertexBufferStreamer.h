#pragma once
#include "VertexBuffer.h"

struct VertexBufferStreamer
{
	UINT vertexBufferCount;
	VertexBuffer** vertexBuffers;
	D3D12_VERTEX_BUFFER_VIEW* vertexBufferViews;

	UINT elementCount;
	D3D12_INPUT_ELEMENT_DESC* elements;
};