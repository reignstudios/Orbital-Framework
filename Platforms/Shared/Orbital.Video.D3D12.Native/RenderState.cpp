#include "RenderState.h"
#include "RenderPass.h"
#include "ConstantBuffer.h"
#include "Texture.h"
#include "Utils.h"

bool TriangleCullingToNative(TriangleCulling cull, D3D12_CULL_MODE* nativeCull)
{
	switch (cull)
	{
		case TriangleCulling::TriangleCulling_None: *nativeCull = D3D12_CULL_MODE::D3D12_CULL_MODE_NONE; break;
		case TriangleCulling::TriangleCulling_Back: *nativeCull = D3D12_CULL_MODE::D3D12_CULL_MODE_BACK; break;
		case TriangleCulling::TriangleCulling_Front: *nativeCull = D3D12_CULL_MODE::D3D12_CULL_MODE_FRONT; break;
		default: return false;
	}
	return true;
}

bool TriangleFillModeToNative(TriangleFillMode mode, D3D12_FILL_MODE* nativeMode)
{
	switch (mode)
	{
		case TriangleFillMode::TriangleFillMode_Solid: *nativeMode = D3D12_FILL_MODE::D3D12_FILL_MODE_SOLID; break;
		case TriangleFillMode::TriangleFillMode_Wireframe: *nativeMode = D3D12_FILL_MODE::D3D12_FILL_MODE_WIREFRAME; break;
		default: return false;
	}
	return true;
}

bool BlendFactorToNative(BlendFactor factor, D3D12_BLEND* nativeFactor)
{
	switch (factor)
	{
		case BlendFactor::BlendFactor_Zero: *nativeFactor = D3D12_BLEND::D3D12_BLEND_ZERO; break;
		case BlendFactor::BlendFactor_One: *nativeFactor = D3D12_BLEND::D3D12_BLEND_ONE; break;
		case BlendFactor::BlendFactor_SourceColor: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_SRC_COLOR; break;
		case BlendFactor::BlendFactor_SourceColorInverse: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_SRC_COLOR; break;
		case BlendFactor::BlendFactor_SourceAlpha: *nativeFactor = D3D12_BLEND::D3D12_BLEND_SRC_ALPHA; break;
		case BlendFactor::BlendFactor_SourceAlphaInverse: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_SRC_ALPHA; break;
		case BlendFactor::BlendFactor_DestinationColor: *nativeFactor = D3D12_BLEND::D3D12_BLEND_DEST_COLOR; break;
		case BlendFactor::BlendFactor_DestinationColorInverse: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_DEST_COLOR; break;
		case BlendFactor::BlendFactor_DestinationAlpha: *nativeFactor = D3D12_BLEND::D3D12_BLEND_DEST_ALPHA; break;
		case BlendFactor::BlendFactor_DestinationAlphaInverse: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_DEST_ALPHA; break;
		case BlendFactor::BlendFactor_SourceAlphaSaturate: *nativeFactor = D3D12_BLEND::D3D12_BLEND_SRC_ALPHA_SAT; break;
		case BlendFactor::BlendFactor_SourceColor2: *nativeFactor = D3D12_BLEND::D3D12_BLEND_SRC1_COLOR; break;
		case BlendFactor::BlendFactor_SourceColorInverse2: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_SRC1_COLOR; break;
		case BlendFactor::BlendFactor_SourceAlpha2: *nativeFactor = D3D12_BLEND::D3D12_BLEND_SRC1_ALPHA; break;
		case BlendFactor::BlendFactor_SourceAlphaInverse2: *nativeFactor = D3D12_BLEND::D3D12_BLEND_INV_SRC1_ALPHA; break;
		default: return false;
	}
	return true;
}

bool BlendOperationToNative(BlendOperation operation, D3D12_BLEND_OP* nativeOperation)
{
	switch (operation)
	{
		case BlendOperation::BlendOperation_Add: *nativeOperation = D3D12_BLEND_OP::D3D12_BLEND_OP_ADD; break;
		case BlendOperation::BlendOperation_Subtract: *nativeOperation = D3D12_BLEND_OP::D3D12_BLEND_OP_SUBTRACT; break;
		case BlendOperation::BlendOperation_SubtractReversed: *nativeOperation = D3D12_BLEND_OP::D3D12_BLEND_OP_REV_SUBTRACT; break;
		case BlendOperation::BlendOperation_Minimum: *nativeOperation = D3D12_BLEND_OP::D3D12_BLEND_OP_MIN; break;
		case BlendOperation::BlendOperation_Maximum: *nativeOperation = D3D12_BLEND_OP::D3D12_BLEND_OP_MAX; break;
		default: return false;
	}
	return true;
}

bool LogicalBlendOperationToNative(LogicalBlendOperation operation, D3D12_LOGIC_OP* nativeOperation)
{
	switch (operation)
	{
		case LogicalBlendOperation::LogicalBlendOperation_Clear: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_CLEAR; break;
		case LogicalBlendOperation::LogicalBlendOperation_Set: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_SET; break;
		case LogicalBlendOperation::LogicalBlendOperation_Copy: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_COPY; break;
		case LogicalBlendOperation::LogicalBlendOperation_CopyInverted: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_COPY_INVERTED; break;
		case LogicalBlendOperation::LogicalBlendOperation_NoOperation: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_NOOP; break;
		case LogicalBlendOperation::LogicalBlendOperation_Invert: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_INVERT; break;
		case LogicalBlendOperation::LogicalBlendOperation_AND: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_AND; break;
		case LogicalBlendOperation::LogicalBlendOperation_NAND: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_NAND; break;
		case LogicalBlendOperation::LogicalBlendOperation_OR: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_OR; break;
		case LogicalBlendOperation::LogicalBlendOperation_NOR: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_NOR; break;
		case LogicalBlendOperation::LogicalBlendOperation_XOR: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_XOR; break;
		case LogicalBlendOperation::LogicalBlendOperation_Equivalent: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_EQUIV; break;
		case LogicalBlendOperation::LogicalBlendOperation_AND_Reverse: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_AND_REVERSE; break;
		case LogicalBlendOperation::LogicalBlendOperation_AND_Inverted: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_AND_INVERTED; break;
		case LogicalBlendOperation::LogicalBlendOperation_OR_Reverse: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_OR_REVERSE; break;
		case LogicalBlendOperation::LogicalBlendOperation_OR_Inverted: *nativeOperation = D3D12_LOGIC_OP::D3D12_LOGIC_OP_OR_INVERTED; break;
		default: return false;
	}
	return true;
}

bool DepthStencilTestFunctionToNative(DepthStencilTestFunction function, D3D12_COMPARISON_FUNC* nativeFunction)
{
	switch (function)
	{
		case DepthStencilTestFunction::DepthStencilTestFunction_Always: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_ALWAYS; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_Never: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_NEVER; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_Equal: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_EQUAL; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_NotEqual: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_NOT_EQUAL; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_LessThan: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_LESS; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_LessThanOrEqual: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_LESS_EQUAL; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_GreaterThan: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_GREATER; break;
		case DepthStencilTestFunction::DepthStencilTestFunction_GreaterThanOrEqual: *nativeFunction = D3D12_COMPARISON_FUNC::D3D12_COMPARISON_FUNC_GREATER_EQUAL; break;
		default: return false;
	}
	return true;
}

bool StencilOperationToNative(StencilOperation operation, D3D12_STENCIL_OP* nativeOperation)
{
	switch (operation)
	{
		case StencilOperation::StencilOperation_Keep: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_KEEP; break;
		case StencilOperation::StencilOperation_Zero: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_ZERO; break;
		case StencilOperation::StencilOperation_Invert: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_INVERT; break;
		case StencilOperation::StencilOperation_IncrementWrap: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_INCR; break;
		case StencilOperation::StencilOperation_DecrementWrap: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_DECR; break;
		case StencilOperation::StencilOperation_IncrementClamp: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_INCR_SAT; break;
		case StencilOperation::StencilOperation_DecrementClamp: *nativeOperation = D3D12_STENCIL_OP::D3D12_STENCIL_OP_DECR_SAT; break;
		default: return false;
	}
	return true;
}

extern "C"
{
	ORBITAL_EXPORT RenderState* Orbital_Video_D3D12_RenderState_Create(Device* device)
	{
		RenderState* handle = (RenderState*)calloc(1, sizeof(RenderState));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderState_Init(RenderState* handle, RenderStateDesc* desc)
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
		pipelineDesc.pRootSignature = shaderEffect->signature.signature;

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
		if (desc->textureCount != 0 || desc->textureDepthStencilCount != 0)
		{
			handle->textureCount = desc->textureCount;
			UINT size = sizeof(Texture*) * desc->textureCount;
			handle->textures = (Texture**)malloc(size);
			memcpy(handle->textures, desc->textures, size);

			handle->textureDepthStencilCount = desc->textureDepthStencilCount;
			size = sizeof(DepthStencil*) * desc->textureDepthStencilCount;
			handle->textureDepthStencils = (DepthStencil**)malloc(size);
			memcpy(handle->textureDepthStencils, desc->textureDepthStencils, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = handle->textureCount + handle->textureDepthStencilCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->textureHeap)))) return 0;
			handle->textureGPUDescHandle = handle->textureHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuTextureHeap = handle->textureHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->textureCount; ++i)
			{
				Texture* texture = (Texture*)desc->textures[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}

			for (int i = 0; i != desc->textureDepthStencilCount; ++i)
			{
				DepthStencil* depthStencil = (DepthStencil*)desc->textureDepthStencils[i];
				D3D12_CPU_DESCRIPTOR_HANDLE heap = depthStencil->shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
				handle->device->device->CopyDescriptorsSimple(1, cpuTextureHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				cpuTextureHeap.ptr += heapSize;
			}
		}

		// add read/write buffer heaps
		if (desc->readWriteBufferCount != 0)
		{
			handle->readWriteBufferCount = desc->readWriteBufferCount;
			UINT size = sizeof(intptr_t) * desc->readWriteBufferCount;
			handle->readWriteBuffers = (intptr_t*)malloc(size);
			memcpy(handle->readWriteBuffers, desc->readWriteBuffers, size);

			D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
			heapDesc.NumDescriptors = handle->readWriteBufferCount;
			heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			if (FAILED(handle->device->device->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(&handle->readWriteBufferHeap)))) return 0;
			handle->readWriteBufferGPUDescHandle = handle->readWriteBufferHeap->GetGPUDescriptorHandleForHeapStart();
			D3D12_CPU_DESCRIPTOR_HANDLE cpuReadWriteBufferHeap = handle->readWriteBufferHeap->GetCPUDescriptorHandleForHeapStart();
			UINT heapSize = handle->device->device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

			for (int i = 0; i != desc->readWriteBufferCount; ++i)
			{
				if (desc->readWriteTypes[i] == ReadWriteBufferType::ReadWriteBufferType_Texture)
				{
					Texture* texture = (Texture*)desc->readWriteBuffers[i];
					D3D12_CPU_DESCRIPTOR_HANDLE heap = texture->shaderResourceHeap->GetCPUDescriptorHandleForHeapStart();
					handle->device->device->CopyDescriptorsSimple(1, cpuReadWriteBufferHeap, heap, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					cpuReadWriteBufferHeap.ptr += heapSize;
				}
				else
				{
					return 0;
				}
			}
		}

		// topology
		if (!GetNative_VertexBufferTopology(desc->vertexBufferTopology, &pipelineDesc.PrimitiveTopologyType)) return 0;
		switch (pipelineDesc.PrimitiveTopologyType)
		{
			case D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_POINT: handle->topology = D3D_PRIMITIVE_TOPOLOGY::D3D_PRIMITIVE_TOPOLOGY_POINTLIST; break;
			case D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_LINE: handle->topology = D3D_PRIMITIVE_TOPOLOGY::D3D_PRIMITIVE_TOPOLOGY_LINELIST; break;
			case D3D12_PRIMITIVE_TOPOLOGY_TYPE::D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE: handle->topology = D3D_PRIMITIVE_TOPOLOGY::D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST; break;
			default: return 0;
		}

		// vertex buffer layout
		VertexBufferStreamer* vertexBufferStreamer = (VertexBufferStreamer*)desc->vertexBufferStreamer;
		handle->vertexBufferStreamer = vertexBufferStreamer;
		pipelineDesc.InputLayout.NumElements = vertexBufferStreamer->elementCount;
		size_t elementBufferSize = sizeof(D3D12_INPUT_ELEMENT_DESC) * vertexBufferStreamer->elementCount;
		pipelineDesc.InputLayout.pInputElementDescs = (D3D12_INPUT_ELEMENT_DESC*)alloca(elementBufferSize);
		memcpy((void*)pipelineDesc.InputLayout.pInputElementDescs, vertexBufferStreamer->elements, elementBufferSize);

		// vertex buffer layout
		handle->indexBuffer = (IndexBuffer*)desc->indexBuffer;
		
		// render targets
		RenderPass* renderPass = (RenderPass*)desc->renderPass;
		if (renderPass->swapChain != NULL)
		{
			pipelineDesc.NumRenderTargets = 1;
			memcpy(pipelineDesc.RTVFormats, &renderPass->swapChain->format, sizeof(DXGI_FORMAT) * pipelineDesc.NumRenderTargets);
		}
		else
		{
			pipelineDesc.NumRenderTargets = renderPass->renderTargetCount;
			memcpy(pipelineDesc.RTVFormats, renderPass->renderTargetFormats, sizeof(DXGI_FORMAT) * pipelineDesc.NumRenderTargets);
		}

		// depth stencil
		pipelineDesc.DSVFormat = renderPass->depthStencilFormat;
		
        pipelineDesc.DepthStencilState.DepthEnable = desc->depthStencilDesc.depthTestEnable && renderPass->depthStencilDesc != NULL;
		pipelineDesc.DepthStencilState.DepthWriteMask = desc->depthStencilDesc.depthWriteEnable ? D3D12_DEPTH_WRITE_MASK_ALL : D3D12_DEPTH_WRITE_MASK_ZERO;
		if (!DepthStencilTestFunctionToNative(desc->depthStencilDesc.depthTestFunction, &pipelineDesc.DepthStencilState.DepthFunc)) return 0;

        pipelineDesc.DepthStencilState.StencilEnable = desc->depthStencilDesc.stencilTestEnable && renderPass->depthStencilDesc != NULL;
		pipelineDesc.DepthStencilState.StencilReadMask = D3D12_DEFAULT_STENCIL_READ_MASK;
        pipelineDesc.DepthStencilState.StencilWriteMask = desc->depthStencilDesc.stencilWriteEnable ? D3D12_DEFAULT_STENCIL_WRITE_MASK : 0;

		D3D12_DEPTH_STENCILOP_DESC stencilOp = {};
		if (!DepthStencilTestFunctionToNative(desc->depthStencilDesc.stencilFrontFacingDesc.stencilTestFunction, &stencilOp.StencilFunc)) return 0;
		if (!StencilOperationToNative(desc->depthStencilDesc.stencilFrontFacingDesc.stencilPassOperation, &stencilOp.StencilPassOp)) return 0;
		if (!StencilOperationToNative(desc->depthStencilDesc.stencilFrontFacingDesc.stencilFailOperation, &stencilOp.StencilFailOp)) return 0;
		if (!StencilOperationToNative(desc->depthStencilDesc.stencilFrontFacingDesc.stencilDepthFailOperation, &stencilOp.StencilDepthFailOp)) return 0;
		pipelineDesc.DepthStencilState.FrontFace = stencilOp;

		if (!DepthStencilTestFunctionToNative(desc->depthStencilDesc.stencilBackFacingDesc.stencilTestFunction, &stencilOp.StencilFunc)) return 0;
		if (!StencilOperationToNative(desc->depthStencilDesc.stencilBackFacingDesc.stencilPassOperation, &stencilOp.StencilPassOp)) return 0;
		if (!StencilOperationToNative(desc->depthStencilDesc.stencilBackFacingDesc.stencilFailOperation, &stencilOp.StencilFailOp)) return 0;
		if (!StencilOperationToNative(desc->depthStencilDesc.stencilBackFacingDesc.stencilDepthFailOperation, &stencilOp.StencilDepthFailOp)) return 0;
		pipelineDesc.DepthStencilState.BackFace = stencilOp;

		// rasterizer state
		if (!TriangleCullingToNative(desc->triangleCulling, &pipelineDesc.RasterizerState.CullMode)) return 0;
		if (!TriangleFillModeToNative(desc->triangleFillMode, &pipelineDesc.RasterizerState.FillMode)) return 0;
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
		pipelineDesc.SampleMask = UINT_MAX;
		pipelineDesc.BlendState.AlphaToCoverageEnable = desc->blendDesc.alphaToCoverageEnable;
        pipelineDesc.BlendState.IndependentBlendEnable = desc->blendDesc.independentBlendEnable;

		for (int i = 0; i != pipelineDesc.NumRenderTargets; ++i)// ensure valid render-target blend settings in case no custom ones present
		{
			D3D12_RENDER_TARGET_BLEND_DESC blendDesc = {};
			blendDesc.BlendEnable = false;
			blendDesc.LogicOpEnable = false;
			blendDesc.SrcBlend = D3D12_BLEND_ZERO;
			blendDesc.DestBlend = D3D12_BLEND_ZERO;
			blendDesc.BlendOp = D3D12_BLEND_OP_ADD;
			blendDesc.SrcBlendAlpha = D3D12_BLEND_ZERO;
			blendDesc.DestBlendAlpha = D3D12_BLEND_ZERO;
			blendDesc.BlendOpAlpha = D3D12_BLEND_OP_ADD;
			blendDesc.LogicOp = D3D12_LOGIC_OP_NOOP;
			blendDesc.RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;
			pipelineDesc.BlendState.RenderTarget[i] = blendDesc;
		}

		for (int i = 0; i != desc->blendDesc.renderTargetBlendDescCount; ++i)// set custom blending states
		{
			auto renderTargetBlendDesc = desc->blendDesc.renderTargetBlendDescs[i];
			D3D12_RENDER_TARGET_BLEND_DESC blendDesc = {};

			blendDesc.BlendEnable = renderTargetBlendDesc.blendingEnabled;
			blendDesc.LogicOpEnable = renderTargetBlendDesc.logicOperationEnabled;

			if (!BlendFactorToNative(renderTargetBlendDesc.sourceFactor, &blendDesc.SrcBlend)) return 0;
			if (!BlendFactorToNative(renderTargetBlendDesc.destinationFactor, &blendDesc.DestBlend)) return 0;
			if (!BlendOperationToNative(renderTargetBlendDesc.operation, &blendDesc.BlendOp)) return 0;

			if (renderTargetBlendDesc.alphaBlendingSeparated)
			{
				if (!BlendFactorToNative(renderTargetBlendDesc.sourceAlphaFactor, &blendDesc.SrcBlendAlpha)) return 0;
				if (!BlendFactorToNative(renderTargetBlendDesc.destinationAlphaFactor, &blendDesc.DestBlendAlpha)) return 0;
				if (!BlendOperationToNative(renderTargetBlendDesc.alphaOperation, &blendDesc.BlendOpAlpha)) return 0;
			}
			else
			{
				blendDesc.SrcBlendAlpha = blendDesc.SrcBlend;
				blendDesc.DestBlendAlpha = blendDesc.DestBlend;
				blendDesc.BlendOpAlpha = blendDesc.BlendOp;
			}

			if (!LogicalBlendOperationToNative(renderTargetBlendDesc.logicalOperation, &blendDesc.LogicOp)) return 0;
			blendDesc.RenderTargetWriteMask = (D3D12_COLOR_WRITE_ENABLE)renderTargetBlendDesc.writeMask;

			pipelineDesc.BlendState.RenderTarget[i] = blendDesc;
		}

		// msaa
		if (renderPass->renderTextures != NULL && renderPass->renderTargetCount != 0)
		{
			pipelineDesc.SampleDesc = renderPass->renderTextures[0]->msaaSampleDesc;
		}
		else
		{
			pipelineDesc.SampleDesc.Count = 1;
			pipelineDesc.SampleDesc.Quality = 0;
		}

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