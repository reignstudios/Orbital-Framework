#pragma once

#pragma once
#include "Device.h"

struct ShaderSignatureConstantBuffer
{
	int registerIndex;
	ShaderEffectResourceUsage usage;// ignore for compute shaders
};

struct ShaderSignatureTexture
{
	int registerIndex;
	ShaderEffectResourceUsage usage;// ignore for compute shaders
};

struct ShaderSignatureSampler
{
	int registerIndex;
	ShaderSamplerFilter filter;
	ShaderSamplerAnisotropy anisotropy;
	ShaderSamplerAddress addressU, addressV, addressW;
	ShaderComparisonFunction comparisonFunction;
	ShaderEffectResourceUsage usage;// ignore for compute shaders
};

struct ShaderSignatureRandomAccessBuffer
{
	int registerIndex;
	ShaderEffectResourceUsage usage;// ignore for compute shaders
};

struct ShaderSignatureDesc
{
	int constantBufferCount, textureCount, samplersCount, randomAccessBufferCount;
	ShaderSignatureConstantBuffer* constantBuffers;
	ShaderSignatureTexture* textures;
	ShaderSignatureSampler* samplers;
	ShaderSignatureRandomAccessBuffer* randomAccessBuffers;
};

struct ShaderSignature
{
	ID3D12RootSignature* signature;

	UINT constantBufferCount;
	ShaderSignatureConstantBuffer* constantBuffers;

	UINT textureCount;
	ShaderSignatureTexture* textures;

	UINT randomAccessBufferCount;
	ShaderSignatureRandomAccessBuffer* randomAccessBuffers;
};

int Orbital_Video_D3D12_ShaderSignature_Init(ShaderSignature* handle, Device* device, ShaderSignatureDesc* desc);
void Orbital_Video_D3D12_ShaderSignature_Dispose(ShaderSignature* handle);