#include "ShaderEffect.h"

extern "C"
{
	ORBITAL_EXPORT ShaderEffect* Orbital_Video_D3D12_ShaderEffect_Create(Device* device)
	{
		ShaderEffect* handle = (ShaderEffect*)calloc(1, sizeof(ShaderEffect));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ShaderEffect_Init(ShaderEffect* handle, Shader* vs, Shader* ps, Shader* hs, Shader* ds, Shader* gs)
	{
		// create desc
		D3D12_ROOT_SIGNATURE_DESC desc = {};
		desc.NumParameters = 0;
		//desc.pParameters = (D3D12_ROOT_PARAMETER*)alloca(sizeof(D3D12_ROOT_PARAMETER) * 1);
		desc.NumStaticSamplers = 0;
		//desc.pStaticSamplers = (D3D12_STATIC_SAMPLER_DESC*)alloca(sizeof(D3D12_STATIC_SAMPLER_DESC) * 1);

		// serialize desc
		ID3DBlob* serializedDesc = NULL;
		ID3DBlob* error = NULL;
		if (FAILED(D3D12SerializeRootSignature(&desc, handle->device->maxRootSignatureVersion, &serializedDesc, &error))) goto FAIL_EXIT;

		// create signature per physical GPU node
		handle->signatures = (ID3D12RootSignature**)calloc(handle->device->nodeCount, sizeof(ID3D12RootSignature*));
		for (UINT i = 0; i != handle->device->nodeCount; ++i)
		{
			 D3D12_ROOT_SIGNATURE_DESC signatureDesc = {};
			 if (FAILED(handle->device->device->CreateRootSignature(i, serializedDesc, handle->device->maxRootSignatureVersion, IID_PPV_ARGS(&handle->signatures[i])))) goto FAIL_EXIT;
		}

		// return success
		return 1;

		// dispose and return failed
		FAIL_EXIT:;
		if (serializedDesc != NULL) serializedDesc->Release();
		if (error != NULL) error->Release();
		return 0;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ShaderEffect_Dispose(ShaderEffect* handle)
	{
		if (handle->signatures != NULL)
		{
			for (UINT i = 0; i != handle->device->nodeCount; ++i)
			{
				if (handle->signatures[i] != NULL)
				{
					handle->signatures[i]->Release();
					handle->signatures[i] = NULL;
				}
			}
			free(handle->signatures);
			handle->signatures = NULL;
		}

		free(handle);
	}
}