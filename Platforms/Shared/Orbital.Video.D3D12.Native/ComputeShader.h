#pragma once
#include "Device.h"

struct ComputeShader
{
	Device* device;
	ID3D12RootSignature* signature;

	UINT constantBufferCount;
	ShaderEffectConstantBuffer* constantBuffers;

	UINT textureCount;
	ShaderEffectTexture* textures;
};