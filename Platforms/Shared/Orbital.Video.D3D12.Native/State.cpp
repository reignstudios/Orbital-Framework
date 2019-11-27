#include "State.h"
#include "Shader.h"
#include "Utils.h"

struct StateDesc
{
	Shader *vs, *ps, *hs, *ds, *gs;
	Topology topology;
	BufferLayout bufferLayout;
	UINT renderTargetCount;
	SurfaceFormat renderTargetFormats[8];
	DepthStencilFormat depthStencilFormat;
	char depthEnable, stencilEnable;
	UINT msaaLevel;
};

extern "C"
{
	ORBITAL_EXPORT State* Orbital_Video_D3D12_State_Create(Device* device)
	{
		State* handle = (State*)calloc(1, sizeof(State));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_State_Init(State* handle, StateDesc* desc)
	{
		D3D12_GRAPHICS_PIPELINE_STATE_DESC pipelineDesc = {};

		// shaders
        pipelineDesc.VS = desc->vs->bytecode;
        pipelineDesc.PS = desc->ps->bytecode;
		pipelineDesc.HS = desc->hs->bytecode;
		pipelineDesc.DS = desc->ds->bytecode;
		pipelineDesc.GS = desc->gs->bytecode;

		// topology
		if (!GetNative_Topology(desc->topology, &pipelineDesc.PrimitiveTopologyType)) return 0;

		// buffer layout
		pipelineDesc.InputLayout.NumElements = desc->bufferLayout.elementCount;
		pipelineDesc.InputLayout.pInputElementDescs = (D3D12_INPUT_ELEMENT_DESC*)alloca(sizeof(D3D12_INPUT_ELEMENT_DESC) * desc->bufferLayout.elementCount);
		for (UINT i = 0; i != desc->bufferLayout.elementCount; ++i)
		{
			BufferLayoutElement element = desc->bufferLayout.elements[i];
			D3D12_INPUT_ELEMENT_DESC elementDesc = {};

			elementDesc.InputSlot = element.streamIndex;
			elementDesc.AlignedByteOffset = element.byteOffset;
			elementDesc.SemanticIndex = element.usageIndex;

			switch (element.type)
			{
				case BufferLayoutElementType::BufferLayoutElementType_Float: elementDesc.Format = DXGI_FORMAT_R32_FLOAT; break;
				case BufferLayoutElementType::BufferLayoutElementType_Float2: elementDesc.Format = DXGI_FORMAT_R32G32_FLOAT; break;
				case BufferLayoutElementType::BufferLayoutElementType_Float3: elementDesc.Format = DXGI_FORMAT_R32G32B32_FLOAT; break;
				case BufferLayoutElementType::BufferLayoutElementType_Float4: elementDesc.Format = DXGI_FORMAT_R32G32B32A32_FLOAT; break;
				case BufferLayoutElementType::BufferLayoutElementType_RGBAx8: elementDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM; break;
				default: return 0;
			}

			switch (element.usage)
			{
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Position: elementDesc.SemanticName = "POSITION"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Color: elementDesc.SemanticName = "COLOR"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_UV: elementDesc.SemanticName = "TEXCOORD"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Normal: elementDesc.SemanticName = "NORMAL"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Tangent: elementDesc.SemanticName = "TANGENT"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Binormal: elementDesc.SemanticName = "BINORMAL"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Index: elementDesc.SemanticName = "BLENDINDICES"; break;
				case BufferLayoutElementUsage::BufferLayoutElementUsage_Weight: elementDesc.SemanticName = "BLENDWEIGHT"; break;
				default: return 0;
			}

			memcpy((void*)&pipelineDesc.InputLayout.pInputElementDescs[i], &elementDesc, sizeof(D3D12_INPUT_ELEMENT_DESC));
		}

		// TODO
        //pipelineDesc.pRootSignature = m_rootSignature.Get();
        //pipelineDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
        //pipelineDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);

		// msaa
        pipelineDesc.SampleMask = UINT_MAX;
        pipelineDesc.SampleDesc.Count = desc->msaaLevel;
		pipelineDesc.SampleDesc.Quality = 0;// default MSAA quality

		// render targets
        pipelineDesc.NumRenderTargets = desc->renderTargetCount;
		for (UINT i = 0; i != 8; ++i)
		{
			if (!GetNative_SurfaceFormat(desc->renderTargetFormats[i], &pipelineDesc.RTVFormats[i])) return 0;
		}

		// depth stencil
		if (!GetNative_DepthStencilFormat(desc->depthStencilFormat, &pipelineDesc.DSVFormat)) return 0;
        pipelineDesc.DepthStencilState.DepthEnable = desc->depthEnable;
        pipelineDesc.DepthStencilState.StencilEnable = desc->stencilEnable;

		// create pipeline state
        if (FAILED(handle->device->device->CreateGraphicsPipelineState(&pipelineDesc, IID_PPV_ARGS(&handle->state)))) return 0;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_State_Dispose(State* handle)
	{
		

		free(handle);
	}
}