#include "ComputeShader.h"

extern "C"
{
	ORBITAL_EXPORT ComputeShader* Orbital_Video_D3D12_ComputeShader_Create(Device* device)
	{
		ComputeShader* handle = (ComputeShader*)calloc(1, sizeof(ComputeShader));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ComputeShader_Init(ComputeShader* handle)
	{
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ComputeShader_Dispose(ComputeShader* handle)
	{
		
	}
}