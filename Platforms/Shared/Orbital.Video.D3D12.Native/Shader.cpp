#include "Shader.h"

extern "C"
{
	ORBITAL_EXPORT Shader* Orbital_Video_D3D12_Shader_Create(Device* device)
	{
		Shader* handle = (Shader*)calloc(1, sizeof(Shader));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_Shader_Init(Shader* handle, BYTE* bytecode, UINT bytecodeLength)
	{
		if (bytecodeLength == 0) return 0;
		handle->bytecode.BytecodeLength = bytecodeLength;
		handle->bytecode.pShaderBytecode = malloc(bytecodeLength);
		memcpy((void*)handle->bytecode.pShaderBytecode, bytecode, bytecodeLength);
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_Shader_Dispose(Shader* handle)
	{
		if (handle->bytecode.pShaderBytecode != NULL)
		{
			free((void*)handle->bytecode.pShaderBytecode);
			handle->bytecode.pShaderBytecode = NULL;
		}

		free(handle);
	}
}