#pragma once
#include "Shader.h"

struct ShaderEffect
{
	Device* device;
	Shader *vs, *ps, *hs, *ds, *gs;

	ID3D12RootSignature* signature;

	UINT constantBufferCount;
	ShaderEffectConstantBuffer* constantBuffers;

	UINT textureCount;
	ShaderEffectTexture* textures;
};