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

struct ShaderSignatureReadWriteBuffer
{
	int registerIndex;
	ShaderEffectResourceUsage usage;// ignore for compute shaders
};

struct ShaderSignatureDesc
{
	int constantBufferCount, textureCount, samplersCount, readWriteBufferCount;
	ShaderSignatureConstantBuffer* constantBuffers;
	ShaderSignatureTexture* textures;
	ShaderSampler* samplers;
	ShaderSignatureReadWriteBuffer* readWriteBuffers;
};

struct ShaderSignature
{
	ID3D12RootSignature* signature;

	UINT constantBufferCount;
	ShaderSignatureConstantBuffer* constantBuffers;

	UINT textureCount;
	ShaderSignatureTexture* textures;

	UINT readWriteBufferCount;
	ShaderSignatureReadWriteBuffer* readWriteBuffers;
};

int Orbital_Video_D3D12_ShaderSignature_Init(ShaderSignature* handle, Device* device, ShaderSignatureDesc* desc);
void Orbital_Video_D3D12_ShaderSignature_Dispose(ShaderSignature* handle);