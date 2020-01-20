#pragma once
#include "Device.h"

struct RenderState
{
	Device* device;
	ID3D12PipelineState* state;
	ID3D12RootSignature* shaderEffectSignature;

	ID3D12DescriptorHeap* constantBufferHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE constantBufferGPUDescHandle;

	ID3D12DescriptorHeap* textureHeap;
	D3D12_GPU_DESCRIPTOR_HANDLE textureGPUDescHandle;

	D3D_PRIMITIVE_TOPOLOGY topology;
	D3D12_VERTEX_BUFFER_VIEW vertexBufferView;
};