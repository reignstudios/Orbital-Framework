#include "RenderState.h"
#include "ShaderEffect.h"
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
        if (shaderEffect->vs != NULL) pipelineDesc.VS = shaderEffect->vs->bytecode;
        if (shaderEffect->ps != NULL) pipelineDesc.PS = shaderEffect->ps->bytecode;
		if (shaderEffect->hs != NULL) pipelineDesc.HS = shaderEffect->hs->bytecode;
		if (shaderEffect->ds != NULL) pipelineDesc.DS = shaderEffect->ds->bytecode;
		if (shaderEffect->gs != NULL) pipelineDesc.GS = shaderEffect->gs->bytecode;
		pipelineDesc.pRootSignature = shaderEffect->signatures[gpuIndex];

		// topology
		if (!GetNative_VertexBufferTopology(desc->vertexBufferTopology, &pipelineDesc.PrimitiveTopologyType)) return 0;

		// vertex buffer layout
		pipelineDesc.InputLayout.NumElements = desc->vertexBufferLayout.elementCount;
		pipelineDesc.InputLayout.pInputElementDescs = (D3D12_INPUT_ELEMENT_DESC*)alloca(sizeof(D3D12_INPUT_ELEMENT_DESC) * desc->vertexBufferLayout.elementCount);
		for (int i = 0; i != desc->vertexBufferLayout.elementCount; ++i)
		{
			VertexBufferLayoutElement element = desc->vertexBufferLayout.elements[i];
			D3D12_INPUT_ELEMENT_DESC elementDesc = {};

			elementDesc.InputSlot = element.streamIndex;
			elementDesc.AlignedByteOffset = element.byteOffset;
			elementDesc.SemanticIndex = element.usageIndex;

			switch (element.type)
			{
				case VertexBufferLayoutElementType::VertexBufferLayoutElementType_Float: elementDesc.Format = DXGI_FORMAT_R32_FLOAT; break;
				case VertexBufferLayoutElementType::VertexBufferLayoutElementType_Float2: elementDesc.Format = DXGI_FORMAT_R32G32_FLOAT; break;
				case VertexBufferLayoutElementType::VertexBufferLayoutElementType_Float3: elementDesc.Format = DXGI_FORMAT_R32G32B32_FLOAT; break;
				case VertexBufferLayoutElementType::VertexBufferLayoutElementType_Float4: elementDesc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT; break;
				case VertexBufferLayoutElementType::VertexBufferLayoutElementType_RGBAx8: elementDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM; break;
				default: return 0;
			}

			switch (element.usage)
			{
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Position: elementDesc.SemanticName = "POSITION"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Color: elementDesc.SemanticName = "COLOR"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_UV: elementDesc.SemanticName = "TEXCOORD"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Normal: elementDesc.SemanticName = "NORMAL"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Tangent: elementDesc.SemanticName = "TANGENT"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Binormal: elementDesc.SemanticName = "BINORMAL"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Index: elementDesc.SemanticName = "BLENDINDICES"; break;
				case VertexBufferLayoutElementUsage::VertexBufferLayoutElementUsage_Weight: elementDesc.SemanticName = "BLENDWEIGHT"; break;
				default: return 0;
			}

			memcpy((void*)&pipelineDesc.InputLayout.pInputElementDescs[i], &elementDesc, sizeof(D3D12_INPUT_ELEMENT_DESC));
		}

		// TODO
        //pipelineDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
        //pipelineDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);

		// msaa
        pipelineDesc.SampleMask = UINT_MAX;
        pipelineDesc.SampleDesc.Count = desc->msaaLevel != 0 ? desc->msaaLevel : 1;
		pipelineDesc.SampleDesc.Quality = 0;// default MSAA quality

		// render targets
        pipelineDesc.NumRenderTargets = desc->renderTargetCount;
		for (int i = 0; i != desc->renderTargetCount; ++i)
		{
			if (!GetNative_TextureFormat(desc->renderTargetFormats[i], &pipelineDesc.RTVFormats[i])) return 0;
		}

		// depth stencil
		if (!GetNative_DepthStencilFormat(desc->depthStencilFormat, &pipelineDesc.DSVFormat)) return 0;
        pipelineDesc.DepthStencilState.DepthEnable = desc->depthEnable;
        pipelineDesc.DepthStencilState.StencilEnable = desc->stencilEnable;

		// create pipeline state
        if (FAILED(handle->device->device->CreateGraphicsPipelineState(&pipelineDesc, IID_PPV_ARGS(&handle->state)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_RenderState_Dispose(RenderState* handle)
	{
		if (handle->state != NULL)
		{
			handle->state->Release();
			handle->state = NULL;
		}

		free(handle);
	}
}