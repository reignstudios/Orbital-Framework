#include "ConstantBuffer.h"

extern "C"
{
	ORBITAL_EXPORT ConstantBuffer* Orbital_Video_D3D12_ConstantBuffer_Create(Device* device)
	{
		ConstantBuffer* handle = (ConstantBuffer*)calloc(1, sizeof(ConstantBuffer));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ConstantBuffer_Init(ConstantBuffer* handle)
	{
		
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ConstantBuffer_Dispose(ConstantBuffer* handle)
	{
		if (handle->heap != NULL)
		{
			handle->heap->Release();
			handle->heap = NULL;
		}

		if (handle->resource != NULL)
		{
			handle->resource->Release();
			handle->resource = NULL;
		}

		free(handle);
	}
}