#include "ComputeState.h"
#include "RenderPass.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT ComputeState* Orbital_Video_D3D12_ComputeState_Create(Device* device)
	{
		ComputeState* handle = (ComputeState*)calloc(1, sizeof(ComputeState));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_ComputeState_Init(ComputeState* handle, ComputeStateDesc* desc)
	{
		D3D12_COMPUTE_PIPELINE_STATE_DESC pipelineDesc = {};
		pipelineDesc.NodeMask = handle->device->fullNodeMask;

		// shaders
		ComputeShader* computeShader = (ComputeShader*)desc->computeShader;
		handle->computeShader = computeShader;
		pipelineDesc.CS = computeShader->bytecode;
		pipelineDesc.pRootSignature = computeShader->signature.signature;

		// init resources
		PipelineStateResourcesDesc resourceDesc = {};
		resourceDesc.constantBufferCount = desc->constantBufferCount;
		resourceDesc.constantBuffers = desc->constantBuffers;

		resourceDesc.textureCount = desc->textureCount;
		resourceDesc.textures = desc->textures;

		resourceDesc.textureDepthStencilCount = desc->textureDepthStencilCount;
		resourceDesc.textureDepthStencils = desc->textureDepthStencils;

		resourceDesc.randomAccessBufferCount = desc->randomAccessBufferCount;
		resourceDesc.randomAccessBuffers = desc->randomAccessBuffers;
		resourceDesc.randomAccessTypes = desc->randomAccessTypes;
		if (!Orbital_Video_D3D12_PipelineStateResources_Init(&handle->resources, handle->device, &resourceDesc)) return 0;

		// create pipeline state
		if (FAILED(handle->device->device->CreateComputePipelineState(&pipelineDesc, IID_PPV_ARGS(&handle->state)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_ComputeState_Dispose(ComputeState* handle)
	{
		Orbital_Video_D3D12_PipelineStateResources_Dispose(&handle->resources);

		if (handle->state != NULL)
		{
			handle->state->Release();
			handle->state = NULL;
		}

		free(handle);
	}
}