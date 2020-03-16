#include "ComputeShader.h"

extern "C"
{
	ORBITAL_EXPORT ComputeShader* Orbital_Video_D3D12_ComputeShader_Create(Device* device)
	{
		ComputeShader* handle = (ComputeShader*)calloc(1, sizeof(ComputeShader));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ComputeShader_Init(ComputeShader* handle, BYTE* bytecode, UINT bytecodeLength)
	{
		
		return 0;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ComputeShader_Dispose(ComputeShader* handle)
	{
		if (handle->constantBuffers != NULL)
		{
			free(handle->constantBuffers);
			handle->constantBuffers = NULL;
		}

		if (handle->textures != NULL)
		{
			free(handle->textures);
			handle->textures = NULL;
		}

		if (handle->signature != NULL)
		{
			handle->signature->Release();
			handle->signature = NULL;
		}

		free(handle);
	}
}