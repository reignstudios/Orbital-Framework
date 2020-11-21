#pragma once
#include "Device.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "DepthStencil.h"

struct PipelineStateResourcesDesc
{
	int constantBufferCount;
	intptr_t* constantBuffers;

	int textureCount;
	intptr_t* textures;

	int textureDepthStencilCount;
	intptr_t* textureDepthStencils;

	int randomAccessBufferCount;
	intptr_t* randomAccessBuffers;
	RandomAccessBufferType* randomAccessTypes;
};

struct PipelineStateResourcesNode
{
	ID3D12DescriptorHeap* bufferHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE constantBufferGPUDescHandle, textureGPUDescHandle, randomAccessBufferGPUDescHandle;
};

struct PipelineStateResources
{
	Device* device;
	PipelineStateResourcesNode* nodes;

	UINT constantBufferCount;
	ConstantBuffer** constantBuffers;

	UINT textureCount;
	Texture** textures;
	UINT textureDepthStencilCount;
	DepthStencil** textureDepthStencils;

	UINT randomAccessBufferCount;
	intptr_t* randomAccessBuffers;
	RandomAccessBufferType* randomAccessTypes;
};

int Orbital_Video_D3D12_PipelineStateResources_Init(PipelineStateResources* handle, Device* device, PipelineStateResourcesDesc* desc);
void Orbital_Video_D3D12_PipelineStateResources_Dispose(PipelineStateResources* handle);