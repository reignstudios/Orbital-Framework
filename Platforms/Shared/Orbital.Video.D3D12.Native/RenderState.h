#pragma once
#include "Device.h"
#include "ShaderEffect.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "VertexBuffer.h"

struct RenderState
{
	Device* device;
	ID3D12PipelineState* state;
	ShaderEffect* shaderEffect;

	UINT constantBufferCount;
	ConstantBuffer** constantBuffers;
	ID3D12DescriptorHeap* constantBufferHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE constantBufferGPUDescHandle;

	UINT textureCount;
	Texture** textures;
	ID3D12DescriptorHeap* textureHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE textureGPUDescHandle;


	D3D_PRIMITIVE_TOPOLOGY topology;
	VertexBuffer* vertexBuffer;
};