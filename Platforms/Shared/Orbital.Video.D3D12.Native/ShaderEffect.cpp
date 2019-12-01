#include "ShaderEffect.h"

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

		// create desc
		D3D12_VERSIONED_ROOT_SIGNATURE_DESC signatureDesc = {};
		signatureDesc.Version = D3D_ROOT_SIGNATURE_VERSION_1;//handle->device->maxRootSignatureVersion;
		signatureDesc.Desc_1_0.NumParameters = 0;
		//signatureDesc.Desc_1_0.pParameters = (D3D12_ROOT_PARAMETER*)alloca(sizeof(D3D12_ROOT_PARAMETER) * 1);
		signatureDesc.Desc_1_0.NumStaticSamplers = 0;
		//signatureDesc.Desc_1_0.pStaticSamplers = (D3D12_STATIC_SAMPLER_DESC*)alloca(sizeof(D3D12_STATIC_SAMPLER_DESC) * 1);

		// serialize desc
		ID3DBlob* serializedDesc = NULL;
		ID3DBlob* error = NULL;
		//if (FAILED(D3D12SerializeRootSignature(&signatureDesc, handle->device->maxRootSignatureVersion, &serializedDesc, &error))) goto FAIL_EXIT;
		if (FAILED(D3D12SerializeVersionedRootSignature(&signatureDesc, &serializedDesc, &error))) goto FAIL_EXIT;

		// create signature per physical GPU node
		handle->signatures = (ID3D12RootSignature**)calloc(handle->device->nodeCount, sizeof(ID3D12RootSignature*));
		for (UINT i = 0; i != handle->device->nodeCount; ++i)
		{
			 if (FAILED(handle->device->device->CreateRootSignature(i, serializedDesc->GetBufferPointer(), serializedDesc->GetBufferSize(), IID_PPV_ARGS(&handle->signatures[i])))) goto FAIL_EXIT;
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