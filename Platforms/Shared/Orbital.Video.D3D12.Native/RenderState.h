#pragma once
#include "Device.h"
#include "ShaderEffect.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "DepthStencil.h"
#include "VertexBufferStreamer.h"
#include "IndexBuffer.h"
#include "PipelineStateResources.h"

struct RenderState
{
	Device* device;
	ID3D12PipelineState* state;
	ShaderEffect* shaderEffect;
	PipelineStateResources resources;

	VertexBufferStreamer* vertexBufferStreamer;
	IndexBuffer* indexBuffer;
	D3D_PRIMITIVE_TOPOLOGY topology;
};