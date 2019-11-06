#include "CommandBuffer.h"

extern "C"
{
	ORBITAL_EXPORT CommandBuffer* Orbital_Video_D3D12_CommandBuffer_Create()
	{
		return (CommandBuffer*)calloc(1, sizeof(CommandBuffer));
	}

	ORBITAL_EXPORT bool Orbital_Video_D3D12_CommandBuffer_Init(CommandBuffer* handle)
	{
		

		return true;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_Dispose(CommandBuffer* handle)
	{
		free(handle);
	}
}