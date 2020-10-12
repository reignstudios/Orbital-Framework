#pragma once
#include "Device.h"
#include "ComputeShader.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "DepthStencil.h"
#include "VertexBufferStreamer.h"
#include "IndexBuffer.h"
#include "PipelineStateResources.h"

struct ComputeState
{
	Device* device;
	ID3D12PipelineState* state;
	ComputeShader* computeShader;
	PipelineStateResources resources;
};