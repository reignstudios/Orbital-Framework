#include "RenderState.h"
#include "RenderPass.h"
#include "ShaderEffect.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "VertexBuffer.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT RenderState* Orbital_Video_D3D12_RenderState_Create(Device* device)
	{
		RenderState* handle = (RenderState*)calloc(1, sizeof(RenderState));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderState_Init(RenderState* handle, RenderStateDesc* desc, UINT gpuIndex)
	{
		D3D12_GRAPHICS_PIPELINE_STATE_DESC pipelineDesc = {};

		// shaders
		ShaderEffect* shaderEffect = (ShaderEffect*)desc->shaderEffect;
		handle->shaderEffect = shaderEffect;
        if (shaderEffect->vs != NULL) pipelineDesc.VS = shaderEffect->vs->bytecode;
        if (shaderEffect->ps != NULL) pipelineDesc.PS = shaderEffect->ps->bytecode;
		if (shaderEffect->hs != NULL) pipelineDesc.HS = shaderEffect->hs->bytecode;
		if (shaderEffect->ds != NULL) pipelineDesc.DS = shaderEffect->ds->bytecode;
		if (shaderEffect->gs != NULL) pipelineDesc.GS = shaderEffect->gs->bytecode;
		pipelineDesc.pRootSignature = shaderEffect->signatures[gpuIndex];

		// add constant buffer heaps
		if (desc->constantBufferCount != 0)
		{
			handle->constantBufferCount = desc->constantBufferCount;
			UINT size = sizeof(ConstantBuffer*) * handle->constantBufferCount;
			handle->constantBuffers = (ConstantBuffer**)malloc(size);
			memcpy(handle->constantBuffers, desc->constantBuffers, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = desc->constantBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->constantBufferHeap)))) return 0;
			handle->constantBufferGPUDescHandle = handle->constantBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuComputerBufferHeap = handle->constantBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			for (int i = 0; i != desc->constantBufferCount; ++i)
			{
				ConstantBuffer* constantBuffer = (ConstantBuffer*)desc->constantBuffers[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = constantBuffer->resourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuComputerBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuComputerBufferHeap.ptr += heapSize;
			}
		}

		// add texture heaps
		if (desc->textureCount != 0)
		{
			handle->textureCount = desc->textureCount;
			UINT size = sizeof(Texture*) * handle->textureCount;
			handle->textures = (Texture**)malloc(size);
			memcpy(handle->textures, desc->textures, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = desc->textureCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->textureHeap)))) return 0;
			handle->textureGPUDescHandle = handle->textureHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuTextureHeap = handle->textureHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			for (int i = 0; i != desc->textureCount; ++i)
			{
				Texture* texture = (Texture*)desc->textures[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->textureHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}
		}

		// topology
		if (!GetNative_VertexBufferTopology(desc->vertexBufferTopology, &pipelineDesc.PrimitiveTopologyType)) return 0;
		switch (pipelineDesc.PrimitiveTopologyType)
		{
			case D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_POINT: handle->topology = D3D_PRIMITIVE_TOPOLOGY::D3D10_PRIMITIVE_TOPOLOGY_POINTLIST; break;
			case D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_LINE: handle->topology = D3D_PRIMITIVE_TOPOLOGY::D3D10_PRIMITIVE_TOPOLOGY_LINELIST; break;
			case D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE: handle->topology = D3D_PRIMITIVE_TOPOLOGY::D3D10_PRIMITIVE_TOPOLOGY_TRIANGLELIST; break;
			default: return 0;
		}

		// vertex buffer layout
		VertexBuffer* vertexBuffer = (VertexBuffer*)desc->vertexBuffer;
		pipelineDesc.InputLayout.NumElements = vertexBuffer->elementCount;
		pipelineDesc.InputLayout.pInputElementDescs = (D3D12_INPUT_ELEMENT_DESC*)alloca(sizeof(D3D12_INPUT_ELEMENT_DESC) * vertexBuffer->elementCount);
		memcpy((void*)pipelineDesc.InputLayout.pInputElementDescs, vertexBuffer->elements, sizeof(D3D12_INPUT_ELEMENT_DESC) * vertexBuffer->elementCount);
		handle->vertexBuffer = vertexBuffer;

		// vertex buffer layout
		handle->indexBuffer = (IndexBuffer*)desc->indexBuffer;
		
		// render targets
		RenderPass* renderPass = (RenderPass*)desc->renderPass;
		if (renderPass->swapChain != NULL)
		{
			pipelineDesc.NumRenderTargets = 1;
			memcpy(pipelineDesc.RTVFormats, &renderPass->swapChain->renderTargetFormat, sizeof(DXGI_FORMAT) * pipelineDesc.NumRenderTargets);
		}
		else
		{
			pipelineDesc.NumRenderTargets = renderPass->renderTargetCount;
			memcpy(pipelineDesc.RTVFormats, renderPass->renderTargetFormats, sizeof(DXGI_FORMAT) * pipelineDesc.NumRenderTargets);
		}

		// depth stencil
		pipelineDesc.DSVFormat = renderPass->depthStencilFormat;

        pipelineDesc.DepthStencilState.DepthEnable = desc->depthEnable && renderPass->depthStencilDesc != NULL;
		pipelineDesc.DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;
		pipelineDesc.DepthStencilState.DepthFunc = D3D12_COMPARISON_FUNC_LESS;

        pipelineDesc.DepthStencilState.StencilEnable = desc->stencilEnable && renderPass->depthStencilDesc != NULL;
		pipelineDesc.DepthStencilState.StencilReadMask = D3D12_DEFAULT_STENCIL_READ_MASK;
        pipelineDesc.DepthStencilState.StencilWriteMask = D3D12_DEFAULT_STENCIL_WRITE_MASK;
		D3D12_DEPTH_STENCILOP_DESC stencilOp = {};
		stencilOp.StencilFailOp = D3D12_STENCIL_OP_KEEP;
		stencilOp.StencilDepthFailOp = D3D12_STENCIL_OP_KEEP;
		stencilOp.StencilPassOp = D3D12_STENCIL_OP_KEEP;
		stencilOp.StencilFunc = D3D12_COMPARISON_FUNC_ALWAYS;
		pipelineDesc.DepthStencilState.FrontFace = stencilOp;
		pipelineDesc.DepthStencilState.BackFace = stencilOp;

		// rasterizer state
		pipelineDesc.RasterizerState.FillMode = D3D12_FILL_MODE_SOLID;
        pipelineDesc.RasterizerState.CullMode = D3D12_CULL_MODE_NONE;
        pipelineDesc.RasterizerState.FrontCounterClockwise = FALSE;
        pipelineDesc.RasterizerState.DepthBias = D3D12_DEFAULT_DEPTH_BIAS;
        pipelineDesc.RasterizerState.DepthBiasClamp = D3D12_DEFAULT_DEPTH_BIAS_CLAMP;
        pipelineDesc.RasterizerState.SlopeScaledDepthBias = D3D12_DEFAULT_SLOPE_SCALED_DEPTH_BIAS;
        pipelineDesc.RasterizerState.DepthClipEnable = TRUE;
        pipelineDesc.RasterizerState.MultisampleEnable = FALSE;
        pipelineDesc.RasterizerState.AntialiasedLineEnable = FALSE;
        pipelineDesc.RasterizerState.ForcedSampleCount = 0;
        pipelineDesc.RasterizerState.ConservativeRaster = D3D12_CONSERVATIVE_RASTERIZATION_MODE_OFF;

		// blend state
		pipelineDesc.BlendState.AlphaToCoverageEnable = FALSE;
        pipelineDesc.BlendState.IndependentBlendEnable = FALSE;
        D3D12_RENDER_TARGET_BLEND_DESC blendDesc = {};
		blendDesc.BlendEnable = false;
		blendDesc.LogicOpEnable = false;
		blendDesc.SrcBlend = D3D12_BLEND_ONE;
		blendDesc.DestBlend = D3D12_BLEND_ZERO;
		blendDesc.BlendOp = D3D12_BLEND_OP_ADD;
		blendDesc.SrcBlendAlpha = D3D12_BLEND_ONE;
		blendDesc.DestBlendAlpha = D3D12_BLEND_ZERO;
		blendDesc.BlendOpAlpha = D3D12_BLEND_OP_ADD;
		blendDesc.LogicOp = D3D12_LOGIC_OP_NOOP;
		blendDesc.RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;
        for (int i = 0; i != pipelineDesc.NumRenderTargets; ++i)
        {
			pipelineDesc.BlendState.RenderTarget[i] = blendDesc;
		}

		// msaa
        pipelineDesc.SampleMask = UINT_MAX;
        pipelineDesc.SampleDesc.Count = desc->msaaLevel != 0 ? desc->msaaLevel : 1;
		pipelineDesc.SampleDesc.Quality = 0;// default MSAA quality

		// create pipeline state
        if (FAILED(handle->device->device->CreateGraphicsPipelineState(&pipelineDesc, IID_PPV_ARGS(&handle->state)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_RenderState_Dispose(RenderState* handle)
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

		if (handle->constantBufferHeap != NULL)
		{
			handle->constantBufferHeap->Release();
			handle->constantBufferHeap = NULL;
		}

		if (handle->textureHeap != NULL)
		{
			handle->textureHeap->Release();
			handle->textureHeap = NULL;
		}

		if (handle->state != NULL)
		{
			handle->state->Release();
			handle->state = NULL;
		}

		free(handle);
	}
}