#include "VertexBuffer.h"

extern "C"
{
	ORBITAL_EXPORT VertexBuffer* Orbital_Video_D3D12_VertexBuffer_Create(Device* device)
	{
		VertexBuffer* handle = (VertexBuffer*)calloc(1, sizeof(VertexBuffer));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_VertexBuffer_Init(VertexBuffer* handle)
	{
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_VertexBuffer_Dispose(VertexBuffer* handle)
	{
		if (handle->vertexBuffer != NULL)
		{
			handle->vertexBuffer->Release();
			handle->vertexBuffer = NULL;
		}

		free(handle);
	}
}