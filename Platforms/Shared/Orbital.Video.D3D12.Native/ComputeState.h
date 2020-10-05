#pragma once
#include "Device.h"
#include "ComputeShader.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "DepthStencil.h"
#include "VertexBufferStreamer.h"
#include "IndexBuffer.h"

struct ComputeState
{
	Device* device;
	ID3D12PipelineState* state;
	ComputeShader* computeShader;

	UINT constantBufferCount;
	ConstantBuffer** constantBuffers;
	ID3D12DescriptorHeap* constantBufferHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE constantBufferGPUDescHandle;

	UINT textureCount;
	Texture** textures;
	UINT textureDepthStencilCount;
	DepthStencil** textureDepthStencils;
	ID3D12DescriptorHeap* textureHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE textureGPUDescHandle;

	UINT readWriteBufferCount;
	intptr_t* readWriteBuffers;
	ID3D12DescriptorHeap* readWriteBufferHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE readWriteBufferGPUDescHandle;
};