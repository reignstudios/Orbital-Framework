#include "ShaderSignature.h"
#include "Utils.h"
#include "Device.h"

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

int Orbital_Video_D3D12_ShaderSignature_Init(ShaderSignature* handle, Device* device, ShaderSignatureDesc* desc)
{
	// set version
	D3D12_VERSIONED_ROOT_SIGNATURE_DESC signatureDesc = {};
	signatureDesc.Version = device->maxRootSignatureVersion;
	signatureDesc.Desc_1_1.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

	// configure samplers
	signatureDesc.Desc_1_1.NumStaticSamplers = desc->samplersCount;
	signatureDesc.Desc_1_1.pStaticSamplers = (D3D12_STATIC_SAMPLER_DESC*)alloca(sizeof(D3D12_STATIC_SAMPLER_DESC) * desc->samplersCount);
	for (int i = 0; i != desc->samplersCount; ++i)
	{
		ShaderSampler sampler = desc->samplers[i];
		D3D12_STATIC_SAMPLER_DESC samplerDesc = {};

		samplerDesc.ShaderRegister = sampler.registerIndex;
		samplerDesc.ShaderVisibility = D3D12_SHADER_VISIBILITY::D3D12_SHADER_VISIBILITY_ALL;
		samplerDesc.MipLODBias = 0;
		samplerDesc.MinLOD = 0;
		samplerDesc.MaxLOD = D3D12_FLOAT32_MAX;
		if (!GetNative_ShaderComparisonFunction(sampler.comparisonFunction, &samplerDesc.ComparisonFunc)) return 0;

		if (!GetNative_ShaderSamplerFilter(sampler.filter, &samplerDesc.Filter, sampler.comparisonFunction != ShaderComparisonFunction_Never)) return 0;
		if (!GetNative_ShaderSamplerAddress(sampler.addressU, &samplerDesc.AddressU)) return 0;
		if (!GetNative_ShaderSamplerAddress(sampler.addressV, &samplerDesc.AddressV)) return 0;
		if (!GetNative_ShaderSamplerAddress(sampler.addressW, &samplerDesc.AddressW)) return 0;
		if (!GetNative_ShaderSamplerAnisotropy(sampler.anisotropy, &samplerDesc.MaxAnisotropy)) return 0;

		memcpy((void*)&signatureDesc.Desc_1_1.pStaticSamplers[i], &samplerDesc, sizeof(D3D12_STATIC_SAMPLER_DESC));
	}

	// configure constant buffers and textures
	signatureDesc.Desc_1_1.NumParameters = 0;
	if (desc->constantBufferCount != 0) ++signatureDesc.Desc_1_1.NumParameters;
	if (desc->textureCount != 0) ++signatureDesc.Desc_1_1.NumParameters;
	if (desc->readWriteBufferCount != 0) ++signatureDesc.Desc_1_1.NumParameters;
	size_t paramSize;
	if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0) paramSize = sizeof(D3D12_ROOT_PARAMETER);
	else paramSize = sizeof(D3D12_ROOT_PARAMETER1);
	signatureDesc.Desc_1_1.pParameters = (D3D12_ROOT_PARAMETER1*)alloca(sizeof(D3D12_ROOT_PARAMETER1) * signatureDesc.Desc_1_1.NumParameters);// PARAMETER1 is the same size as PARAMETER

	int parameterIndex = 0;
	if (desc->constantBufferCount != 0)
	{
		handle->constantBufferCount = desc->constantBufferCount;
		size_t size = sizeof(ShaderSignatureConstantBuffer) * desc->constantBufferCount;
		handle->constantBuffers = (ShaderSignatureConstantBuffer*)malloc(size);
		memcpy(handle->constantBuffers, desc->constantBuffers, size);

		auto constantBuffers = desc->constantBuffers;
		D3D12_ROOT_PARAMETER1 parameter = {};
		parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
		if (!ResourceUsageToNative(constantBuffers[0].usage, &parameter.ShaderVisibility)) return 0;// get first visibility
		for (int i = 0; i != desc->constantBufferCount; ++i)
		{
			D3D12_SHADER_VISIBILITY visibility;
			if (!ResourceUsageToNative(constantBuffers[i].usage, &visibility)) return 0;
			if (parameter.ShaderVisibility != visibility)// if other resources don't match change to ALL
			{
				parameter.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;
				break;
			}
		}

		parameter.DescriptorTable.NumDescriptorRanges = desc->constantBufferCount;
		if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0) size = sizeof(D3D12_DESCRIPTOR_RANGE);
		else size = sizeof(D3D12_DESCRIPTOR_RANGE1);
		parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE1*)alloca(size * desc->constantBufferCount);
		for (int i = 0; i != desc->constantBufferCount; ++i)
		{
			D3D12_DESCRIPTOR_RANGE1 range = {};
			range.NumDescriptors = 1;
			range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
			range.BaseShaderRegister = constantBuffers[i].registerIndex;
			range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_STATIC;// allows driver to get better performance
			range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0)
			{
				D3D12_DESCRIPTOR_RANGE* oldRange = (D3D12_DESCRIPTOR_RANGE*)parameter.DescriptorTable.pDescriptorRanges;
				memcpy((void*)&oldRange[i], &range, sizeof(D3D12_DESCRIPTOR_RANGE));
			}
			else
			{
				memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[i], &range, sizeof(D3D12_DESCRIPTOR_RANGE1));
			}
		}
		memcpy((void*)&signatureDesc.Desc_1_1.pParameters[parameterIndex], &parameter, paramSize);
		++parameterIndex;
	}

	if (desc->textureCount != 0)
	{
		handle->textureCount = desc->textureCount;
		size_t size = sizeof(ShaderSignatureTexture) * desc->textureCount;
		handle->textures = (ShaderSignatureTexture*)malloc(size);
		memcpy(handle->textures, desc->textures, size);

		auto textures = desc->textures;
		D3D12_ROOT_PARAMETER1 parameter = {};
		parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
		if (!ResourceUsageToNative(textures[0].usage, &parameter.ShaderVisibility)) return 0;// get first visibility
		for (int i = 0; i != desc->textureCount; ++i)
		{
			D3D12_SHADER_VISIBILITY visibility;
			if (!ResourceUsageToNative(textures[i].usage, &visibility)) return 0;
			if (parameter.ShaderVisibility != visibility)// if other resources don't match change to ALL
			{
				parameter.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;
				break;
			}
		}

		parameter.DescriptorTable.NumDescriptorRanges = desc->textureCount;
		if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0) size = sizeof(D3D12_DESCRIPTOR_RANGE);
		else size = sizeof(D3D12_DESCRIPTOR_RANGE1);
		parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE1*)alloca(size * desc->textureCount);
		for (int i = 0; i != desc->textureCount; ++i)
		{
			D3D12_DESCRIPTOR_RANGE1 range = {};
			range.NumDescriptors = 1;
			range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
			range.BaseShaderRegister = textures[i].registerIndex;
			range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_STATIC;// allows driver to get better performance
			range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0)
			{
				D3D12_DESCRIPTOR_RANGE* oldRange = (D3D12_DESCRIPTOR_RANGE*)parameter.DescriptorTable.pDescriptorRanges;
				memcpy((void*)&oldRange[i], &range, sizeof(D3D12_DESCRIPTOR_RANGE));
			}
			else
			{
				memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[i], &range, sizeof(D3D12_DESCRIPTOR_RANGE1));
			}
		}
		memcpy((void*)&signatureDesc.Desc_1_1.pParameters[parameterIndex], &parameter, paramSize);
		++parameterIndex;
	}

	if (desc->readWriteBufferCount != 0)
	{
		handle->readWriteBufferCount = desc->readWriteBufferCount;
		size_t size = sizeof(ShaderSignatureReadWriteBuffer) * desc->readWriteBufferCount;
		handle->readWriteBuffers = (ShaderSignatureReadWriteBuffer*)malloc(size);
		memcpy(handle->readWriteBuffers, desc->readWriteBuffers, size);

		auto readWriteBuffers = desc->readWriteBuffers;
		D3D12_ROOT_PARAMETER1 parameter = {};
		parameter.ParameterType = D3D12_ROOT_PARAMETER_TYPE::D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
		parameter.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;

		parameter.DescriptorTable.NumDescriptorRanges = desc->readWriteBufferCount;
		if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0) size = sizeof(D3D12_DESCRIPTOR_RANGE);
		else size = sizeof(D3D12_DESCRIPTOR_RANGE1);
		parameter.DescriptorTable.pDescriptorRanges = (D3D12_DESCRIPTOR_RANGE1*)alloca(size * desc->readWriteBufferCount);
		for (int i = 0; i != desc->readWriteBufferCount; ++i)
		{
			D3D12_DESCRIPTOR_RANGE1 range = {};
			range.NumDescriptors = 1;
			range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE::D3D12_DESCRIPTOR_RANGE_TYPE_UAV;
			range.BaseShaderRegister = readWriteBuffers[i].registerIndex;
			range.Flags = D3D12_DESCRIPTOR_RANGE_FLAG_DATA_VOLATILE;
			range.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			if (signatureDesc.Version == D3D_ROOT_SIGNATURE_VERSION::D3D_ROOT_SIGNATURE_VERSION_1_0)
			{
				D3D12_DESCRIPTOR_RANGE* oldRange = (D3D12_DESCRIPTOR_RANGE*)parameter.DescriptorTable.pDescriptorRanges;
				memcpy((void*)&oldRange[i], &range, sizeof(D3D12_DESCRIPTOR_RANGE));
			}
			else
			{
				memcpy((void*)&parameter.DescriptorTable.pDescriptorRanges[i], &range, sizeof(D3D12_DESCRIPTOR_RANGE1));
			}
		}
		memcpy((void*)&signatureDesc.Desc_1_1.pParameters[parameterIndex], &parameter, paramSize);
		++parameterIndex;
	}

	// serialize desc
	ID3DBlob* serializedDesc = NULL;
	ID3DBlob* error = NULL;
	int result = 1;
	if (FAILED(D3D12SerializeVersionedRootSignature(&signatureDesc, &serializedDesc, &error))) result = 0;

	// create signature per physical GPU node
	if (result == 1 && FAILED(device->device->CreateRootSignature(0, serializedDesc->GetBufferPointer(), serializedDesc->GetBufferSize(), IID_PPV_ARGS(&handle->signature)))) result = 0;

	// dispose and return failed
	if (serializedDesc != NULL) serializedDesc->Release();
	if (error != NULL)
	{
		OutputDebugStringA((char*)error->GetBufferPointer());
		error->Release();
	}
	return result;
}

void Orbital_Video_D3D12_ShaderSignature_Dispose(ShaderSignature* handle)
{
	if (handle->constantBuffers != NULL)
	{
		free(handle->constantBuffers);
		handle->constantBuffers = NULL;
	}

	if (handle->textures != NULL)
	{
		free(handle->textures);
		handle->textures = NULL;
	}

	if (handle->readWriteBuffers != NULL)
	{
		free(handle->readWriteBuffers);
		handle->readWriteBuffers = NULL;
	}

	if (handle->signature != NULL)
	{
		handle->signature->Release();
		handle->signature = NULL;
	}
}