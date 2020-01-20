#include "ShaderEffect.h"

extern "C"
{
	bool SamplerAddressToNative(ShaderEffectSamplerAddress address, D3D12_TEXTURE_ADDRESS_MODE* nativeAddress)
	{
		switch (address)
		{
			case ShaderEffectSamplerAddress::ShaderEffectSamplerAddress_Wrap:
				*nativeAddress = D3D12_TEXTURE_ADDRESS_MODE::D3D12_TEXTURE_ADDRESS_MODE_WRAP;
				break;

			case ShaderEffectSamplerAddress::ShaderEffectSamplerAddress_Clamp:
				*nativeAddress = D3D12_TEXTURE_ADDRESS_MODE::D3D12_TEXTURE_ADDRESS_MODE_CLAMP;
				break;
			default: return false;
		}
		return true;
	}

	bool SamplerAnisotropyToNative(ShaderEffectSamplerAnisotropy anisotropy, UINT* nativeAnisotropy)
	{
		if (anisotropy == ShaderEffectSamplerAnisotropy::ShaderEffectSamplerAnisotropy_Default) *nativeAnisotropy = 16;
		else *nativeAnisotropy = (UINT)anisotropy;
		return *nativeAnisotropy >= 1 && *nativeAnisotropy <= 16;
	}

	bool SamplerFilterToNative(ShaderEffectSamplerFilter filter, D3D12_FILTER* nativeFilter)
	{
		switch (filter)
		{
			case ShaderEffectSamplerFilter::ShaderEffectSamplerFilter_Point:
				*nativeFilter = D3D12_FILTER::D3D12_FILTER_MIN_MAG_MIP_POINT;
				break;

			case ShaderEffectSamplerFilter::ShaderEffectSamplerFilter_Bilinear:
				*nativeFilter = D3D12_FILTER::D3D12_FILTER_MIN_MAG_LINEAR_MIP_POINT;
				break;

			case ShaderEffectSamplerFilter::ShaderEffectSamplerFilter_Default:
			case ShaderEffectSamplerFilter::ShaderEffectSamplerFilter_Trilinear:
				*nativeFilter = D3D12_FILTER::D3D12_FILTER_MIN_MAG_MIP_LINEAR;
				break;
			default: return false;
		}
		return true;
	}

	bool ResourceUsageToNative(ShaderEffectResourceUsage usage, D3D12_SHADER_VISIBILITY* nativeUsage)
	{
		switch (usage)
		{
			case ShaderEffectResourceUsage::ShaderEffectResourceUsage_VS: *nativeUsage = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_VERTEX; break;
			case ShaderEffectResourceUsage::ShaderEffectResourceUsage_PS: *nativeUsage = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_PIXEL; break;
			case ShaderEffectResourceUsage::ShaderEffectResourceUsage_HS: *nativeUsage = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_HULL; break;
			case ShaderEffectResourceUsage::ShaderEffectResourceUsage_DS: *nativeUsage = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_DOMAIN; break;
			case ShaderEffectResourceUsage::ShaderEffectResourceUsage_GS: *nativeUsage = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_GEOMETRY; break;
			case ShaderEffectResourceUsage::ShaderEffectResourceUsage_All: *nativeUsage = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_ALL; break;
			default: return false;
		}
		return true;
	}

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
		signatureDesc.Desc_1_0.NumStaticSamplers = desc->samplersCount;
		signatureDesc.Desc_1_0.pStaticSamplers = (D3D12_STATIC_SAMPLER_DESC*)alloca(sizeof(D3D12_STATIC_SAMPLER_DESC) * desc->samplersCount);
		for (int i = 0; i != desc->samplersCount; ++i)
		{
			ShaderEffectSampler sampler = desc->samplers[i];
			D3D12_STATIC_SAMPLER_DESC samplerDesc = {};

			samplerDesc.ShaderRegister = sampler.registerIndex;
			samplerDesc.ShaderVisibility = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_ALL;
			samplerDesc.MinLOD = 0;
			samplerDesc.MaxLOD = D3D12_FLOAT32_MAX;
			samplerDesc.ComparisonFunc = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_NEVER;// TODO: add option for comparison. 'SamplerFilterToNative' needs to be updated for this too

			if (!SamplerFilterToNative(sampler.filter, &samplerDesc.Filter)) return 0;
			if (!SamplerAddressToNative(sampler.addressU, &samplerDesc.AddressU)) return 0;
			if (!SamplerAddressToNative(sampler.addressV, &samplerDesc.AddressV)) return 0;
			if (!SamplerAddressToNative(sampler.addressW, &samplerDesc.AddressW)) return 0;
			if (!SamplerAnisotropyToNative(sampler.anisotropy, &samplerDesc.MaxAnisotropy)) return 0;

			memcpy((void*)&signatureDesc.Desc_1_0.pStaticSamplers[i], &samplerDesc, sizeof(D3D12_STATIC_SAMPLER_DESC));
		}

		// configure constant buffers and textures
		signatureDesc.Desc_1_0.NumParameters = desc->constantBufferCount + desc->textureCount;
		signatureDesc.Desc_1_0.pParameters = (D3D12_ROOT_PARAMETER*)alloca(sizeof(D3D12_ROOT_PARAMETER) * signatureDesc.Desc_1_0.NumParameters);
		
		for (int i = 0; i != desc->constantBufferCount; ++i)
		{
			auto constantBuffers = desc->constantBuffers;
			if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0)
			{
				D3D12_ROOT_PARAMETER parameter = {};
				parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
				ResourceUsageToNative(constantBuffers[i].usage, &parameter.ShaderVisibility);

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
				ResourceUsageToNative(constantBuffers[i].usage, &parameter.ShaderVisibility);

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

		for (int i = desc->constantBufferCount, i2 = 0; i != signatureDesc.Desc_1_0.NumParameters; ++i, ++i2)
		{
			auto textures = desc->textures;
			if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0)
			{
				D3D12_ROOT_PARAMETER parameter = {};
				parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
				ResourceUsageToNative(textures[i2].usage, &parameter.ShaderVisibility);

				parameter.DescriptorTable.NumDescriptorRanges = 1;
				parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE*)alloca(sizeof(D3D12_DESCRIPTOR_RANGE) * 1);
				D3D12_DESCRIPTOR_RANGE range = {};
				range.NumDescriptors = 1;
				range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
				range.BaseShaderRegister = i2;
				range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
				memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[0], &range, sizeof(D3D12_DESCRIPTOR_RANGE));
				memcpy((void*)&signatureDesc.Desc_1_0.pParameters[i], &parameter, sizeof(D3D12_ROOT_PARAMETER));
			}
			else
			{
				D3D12_ROOT_PARAMETER1 parameter = {};
				parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
				ResourceUsageToNative(textures[i2].usage, &parameter.ShaderVisibility);

				parameter.DescriptorTable.NumDescriptorRanges = 1;
				parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE1*)alloca(sizeof(D3D12_DESCRIPTOR_RANGE1) * 1);
				D3D12_DESCRIPTOR_RANGE1 range = {};
				range.NumDescriptors = 1;
				range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
				range.BaseShaderRegister = i2;
				range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_STATIC;// allows driver to get better performance
				range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
				memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[0], &range, sizeof(D3D12_DESCRIPTOR_RANGE1));
				memcpy((void*)&signatureDesc.Desc_1_0.pParameters[i], &parameter, sizeof(D3D12_ROOT_PARAMETER1));
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