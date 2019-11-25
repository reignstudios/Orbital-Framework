#include "State.h"
#include "Shader.h"

extern "C"
{
	ORBITAL_EXPORT State* Orbital_Video_D3D12_State_Create(Device* device)
	{
		State* handle = (State*)calloc(1, sizeof(State));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_State_Init(State* handle)
	{
		
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_State_Dispose(State* handle)
	{
		

		free(handle);
	}
}