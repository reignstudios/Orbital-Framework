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
			elementDesc.InputSlotClass = D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA;
			elementDesc.InstanceDataStepRate = 0;

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
        for (int i = 0; i != D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT; ++i)//D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT
        {
			pipelineDesc.BlendState.RenderTarget[i] = blendDesc;
		}

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
		pipelineDesc.DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;
		pipelineDesc.DepthStencilState.DepthFunc = D3D12_COMPARISON_FUNC_LESS;

        pipelineDesc.DepthStencilState.StencilEnable = desc->stencilEnable;
		pipelineDesc.DepthStencilState.StencilReadMask = D3D12_DEFAULT_STENCIL_READ_MASK;
        pipelineDesc.DepthStencilState.StencilWriteMask = D3D12_DEFAULT_STENCIL_WRITE_MASK;
		D3D12_DEPTH_STENCILOP_DESC stencilOp = {};
		stencilOp.StencilFailOp = D3D12_STENCIL_OP_KEEP;
		stencilOp.StencilDepthFailOp = D3D12_STENCIL_OP_KEEP;
		stencilOp.StencilPassOp = D3D12_STENCIL_OP_KEEP;
		stencilOp.StencilFunc = D3D12_COMPARISON_FUNC_ALWAYS;
		pipelineDesc.DepthStencilState.FrontFace = stencilOp;
		pipelineDesc.DepthStencilState.BackFace = stencilOp;

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