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

		// set version
		D3D12_VERSIONED_ROOT_SIGNATURE_DESC signatureDesc = {};
		signatureDesc.Version = handle->device->maxRootSignatureVersion;
		signatureDesc.Desc_1_0.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

		// configure samplers
		signatureDesc.Desc_1_0.NumStaticSamplers = 0;
		//signatureDesc.Desc_1_0.pStaticSamplers = (D3D12_STATIC_SAMPLER_DESC*)alloca(sizeof(D3D12_STATIC_SAMPLER_DESC) * 1);

		// configure constant buffers
		signatureDesc.Desc_1_0.NumParameters = desc->constantBufferCount;
		if (desc->constantBufferCount != 0)
		{
			signatureDesc.Desc_1_0.pParameters = (D3D12_ROOT_PARAMETER*)alloca(sizeof(D3D12_ROOT_PARAMETER) * desc->constantBufferCount);
			for (int i = 0; i != desc->constantBufferCount; ++i)
			{
				if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0)
				{
					D3D12_ROOT_PARAMETER parameter = {};
					parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
					parameter.ShaderVisibility = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_ALL;

					parameter.DescriptorTable.NumDescriptorRanges = 1;
					parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE*)alloca(sizeof(D3D12_DESCRIPTOR_RANGE) * 1);
					D3D12_DESCRIPTOR_RANGE range = {};
					range.NumDescriptors = 1;
					range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
					range.BaseShaderRegister = i;
					range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
					memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[0], &range, sizeof(D3D12_DESCRIPTOR_RANGE));
					memcpy((void*)&signatureDesc.Desc_1_0.pParameters[i], &parameter, sizeof(D3D12_ROOT_PARAMETER));
				}
				else
				{
					D3D12_ROOT_PARAMETER1 parameter = {};
					parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
					parameter.ShaderVisibility = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_ALL;

					parameter.DescriptorTable.NumDescriptorRanges = 1;
					parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE1*)alloca(sizeof(D3D12_DESCRIPTOR_RANGE1) * 1);
					D3D12_DESCRIPTOR_RANGE1 range = {};
					range.NumDescriptors = 1;
					range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
					range.BaseShaderRegister = i;
					range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_STATIC;// allows driver to get better performance
					range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
					memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[0], &range, sizeof(D3D12_DESCRIPTOR_RANGE1));
					memcpy((void*)&signatureDesc.Desc_1_0.pParameters[i], &parameter, sizeof(D3D12_ROOT_PARAMETER1));
				}
			}
		}

		// serialize desc
		ID3DBlob* serializedDesc = NULL;
		ID3DBlob* error = NULL;
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
		if (error != NULL)
		{
			OutputDebugStringA((char*)error->GetBufferPointer());
			error->Release();
		}
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