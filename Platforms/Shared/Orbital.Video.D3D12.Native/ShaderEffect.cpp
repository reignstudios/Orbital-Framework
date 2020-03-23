#include "ShaderEffect.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT ShaderEffect* Orbital_Video_D3D12_ShaderEffect_Create(Device* device)
	{
		ShaderEffect* handle = (ShaderEffect*)calloc(1, sizeof(ShaderEffect));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ShaderEffect_Init(ShaderEffect* handle, Shader* vs, Shader* ps, Shader* hs, Shader* ds, Shader* gs, ShaderEffectDesc* desc)
	{
		// reference shaders
		handle->vs = vs;
		handle->ps = ps;
		handle->hs = hs;
		handle->ds = ds;
		handle->gs = gs;

		// init signature
		return Orbital_Video_D3D12_ShaderSignature_Init(&handle->signature, handle->device, (ShaderSignatureDesc*)desc);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ShaderEffect_Dispose(ShaderEffect* handle)
	{
		Orbital_Video_D3D12_ShaderSignature_Dispose(&handle->signature);
		free(handle);
	}
}