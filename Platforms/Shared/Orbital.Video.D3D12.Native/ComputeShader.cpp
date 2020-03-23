#include "ComputeShader.h"

extern "C"
{
	ORBITAL_EXPORT ComputeShader* Orbital_Video_D3D12_ComputeShader_Create(Device* device)
	{
		ComputeShader* handle = (ComputeShader*)calloc(1, sizeof(ComputeShader));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ComputeShader_Init(ComputeShader* handle, BYTE* bytecode, UINT bytecodeLength, ComputeShaderDesc* desc)
	{
		// reference computer shader object
		if (bytecodeLength == 0) return 0;
		handle->bytecode.BytecodeLength = bytecodeLength;
		handle->bytecode.pShaderBytecode = malloc(bytecodeLength);
		memcpy((void*)handle->bytecode.pShaderBytecode, bytecode, bytecodeLength);

		// init signature
		ShaderSignatureDesc signature = {};

		signature.constantBufferCount = desc->constantBufferCount;
		signature.constantBuffers = (ShaderSignatureConstantBuffer*)alloca(desc->constantBufferCount * sizeof(ShaderSignatureConstantBuffer));
		for (int i = 0; i != desc->constantBufferCount; ++i)
		{
			signature.constantBuffers[i].registerIndex = desc->constantBuffers[i].registerIndex;
			signature.constantBuffers[i].usage = ShaderEffectResourceUsage::ShaderEffectResourceUsage_All;
		}

		signature.textureCount = desc->textureCount;
		signature.textures = (ShaderSignatureTexture*)alloca(desc->textureCount * sizeof(ShaderSignatureTexture));
		for (int i = 0; i != desc->textureCount; ++i)
		{
			signature.textures[i].registerIndex = desc->textures[i].registerIndex;
			signature.textures[i].usage = ShaderEffectResourceUsage::ShaderEffectResourceUsage_All;
		}

		signature.samplersCount = desc->samplersCount;
		signature.samplers = desc->samplers;

		signature.readWriteBufferCount = desc->readWriteBufferCount;
		signature.readWriteBuffers = (ShaderSignatureReadWriteBuffer*)alloca(desc->readWriteBufferCount * sizeof(ShaderSignatureReadWriteBuffer));
		for (int i = 0; i != desc->readWriteBufferCount; ++i)
		{
			signature.readWriteBuffers[i].registerIndex = desc->readWriteBuffers[i].registerIndex;
			signature.readWriteBuffers[i].usage = ShaderEffectResourceUsage::ShaderEffectResourceUsage_All;
		}

		return Orbital_Video_D3D12_ShaderSignature_Init(&handle->signature, handle->device, &signature);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ComputeShader_Dispose(ComputeShader* handle)
	{
		if (handle->bytecode.pShaderBytecode != NULL)
		{
			free((void*)handle->bytecode.pShaderBytecode);
			handle->bytecode.pShaderBytecode = NULL;
		}

		Orbital_Video_D3D12_ShaderSignature_Dispose(&handle->signature);
		free(handle);
	}
}