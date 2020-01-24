#pragma once
#include "Shader.h"

struct ShaderEffect
{
	Device* device;
	Shader *vs, *ps, *hs, *ds, *gs;

	UINT signatureCount;
	ID3D12RootSignature** signatures;// signature per GPU node

	UINT constantBufferCount;
	ShaderEffectConstantBuffer* constantBuffers;

	UINT textureCount;
	ShaderEffectTexture* textures;
};