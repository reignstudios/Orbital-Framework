#include "RenderPass.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT RenderPass* Orbital_Video_D3D12_RenderPass_Create(Device* device)
	{
		RenderPass* handle = (RenderPass*)calloc(1, sizeof(RenderPass));
		handle->device = device;
		return handle;
	}

	int Orbital_Video_D3D12_RenderPass_Init_Base(RenderPass* handle, RenderPassDesc* desc, RenderTextureUsage* usages, StencilUsage stencilUsage, bool onlyUseFirstRenderTargetDesc)
	{
		// create nodes
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			// render-pass: render target
			for (UINT i = 0, d = 0; i != handle->renderTargetCount; ++i)
			{
				// determine what access type to use
				D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE accessType;
				if (desc->renderTargetDescs[d].clearColor)
				{
					accessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
				}
				else if (usages != NULL)
				{
					switch (usages[d])
					{
						case RenderTextureUsage::RenderTextureUsage_Discard: accessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_DISCARD; break;
						case RenderTextureUsage::RenderTextureUsage_Preserve: accessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_PRESERVE; break;
						default: return 0;
					}
				}
				else
				{
					accessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_DISCARD;
				}

				// begin
				handle->nodes[n].renderTargetDescs[i].BeginningAccess.Type = accessType;
				memcpy(handle->nodes[n].renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Color, desc->renderTargetDescs[d].clearColorValue, sizeof(float) * 4);
				handle->nodes[n].renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Format = handle->renderTargetFormats[i];

				// end
				handle->nodes[n].renderTargetDescs[i].EndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
				//handle->renderTargetDescs[i].EndingAccess.Resolve// TODO: if MSAA: D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_RESOLVE

				if (!onlyUseFirstRenderTargetDesc) ++d;
			}

			// render-pass: depth stencil
			if (handle->depthStencil != NULL)
			{
				handle->depthStencilFormat = handle->depthStencil->format;

				handle->nodes[n].depthStencilDesc = (D3D12_RENDER_PASS_DEPTH_STENCIL_DESC*)calloc(1, sizeof(D3D12_RENDER_PASS_DEPTH_STENCIL_DESC));
				handle->nodes[n].depthStencilDesc->cpuDescriptor = handle->depthStencil->nodes[n].depthStencilResourceCPUHeapHandle;

				// determine what depth access type to use
				D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE depthAccessType;
				if (desc->depthStencilDesc.clearDepth) depthAccessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
				else depthAccessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_PRESERVE;

				// determine what stencil access type to use
				D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE stencilAccessType;
				if (desc->depthStencilDesc.clearStencil)
				{
					stencilAccessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
				}
				else
				{
					switch (stencilUsage)
					{
						case StencilUsage::StencilUsage_Discard: stencilAccessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_DISCARD; break;
						case StencilUsage::StencilUsage_Preserve: stencilAccessType = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_PRESERVE; break;
						default: return 0;
					}
				}

				// begin depth
				handle->nodes[n].depthStencilDesc->DepthBeginningAccess.Type = depthAccessType;
				handle->nodes[n].depthStencilDesc->DepthBeginningAccess.Clear.ClearValue.DepthStencil.Depth = desc->depthStencilDesc.depthValue;
				handle->nodes[n].depthStencilDesc->DepthBeginningAccess.Clear.ClearValue.Format = handle->depthStencil->format;

				// end depth
				handle->nodes[n].depthStencilDesc->DepthEndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
				//handle->depthStencilDesc->DepthEndingAccess.Resolve// ??

				// begin stencil
				handle->nodes[n].depthStencilDesc->StencilBeginningAccess.Type = stencilAccessType;
				handle->nodes[n].depthStencilDesc->StencilBeginningAccess.Clear.ClearValue.DepthStencil.Stencil = (UINT8)(desc->depthStencilDesc.stencilValue * UINT8_MAX);
				handle->nodes[n].depthStencilDesc->StencilBeginningAccess.Clear.ClearValue.Format = handle->depthStencil->format;

				// end stencil
				handle->nodes[n].depthStencilDesc->StencilEndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
				//handle->depthStencilDesc->StencilEndingAccess.Resolve// ??
			}
		}

		return 1;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init_WithSwapChain(RenderPass* handle, RenderPassDesc* desc, SwapChain* swapChain, DepthStencil* depthStencil, StencilUsage stencilUsage)
	{
		handle->renderTargetCount = swapChain->bufferCount;

		// copy render-target formats
		handle->renderTargetFormats = (DXGI_FORMAT*)calloc(swapChain->bufferCount, sizeof(DXGI_FORMAT));
		for (UINT i = 0; i != swapChain->bufferCount; ++i)
		{
			handle->renderTargetFormats[i] = swapChain->format;
		}

		// create nodes
		handle->nodes = (RenderPassNode*)calloc(handle->device->nodeCount, sizeof(RenderPassNode));
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			if (swapChain->type == SwapChainType::SwapChainType_SingleGPU_Standard)
			{
				handle->nodes[n].renderTargetResources = (ID3D12Resource**)calloc(swapChain->bufferCount, sizeof(ID3D12Resource));
				handle->nodes[n].renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(swapChain->bufferCount, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
				for (UINT i = 0; i != swapChain->bufferCount; ++i)
				{
					handle->nodes[n].renderTargetResources[i] = swapChain->resources[i];
					handle->nodes[n].renderTargetDescs[i].cpuDescriptor = swapChain->nodes[n].resourceDescCPUHandle;
				}
			}
			else if (swapChain->type == SwapChainType::SwapChainType_MultiGPU_AFR)
			{
				handle->nodes[n].renderTargetResources = (ID3D12Resource**)calloc(1, sizeof(ID3D12Resource));
				handle->nodes[n].renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(1, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
				handle->nodes[n].renderTargetResources[0] = swapChain->resources[n];
				handle->nodes[n].renderTargetDescs[0].cpuDescriptor = swapChain->nodes[n].resourceDescCPUHandle;
			}
			else
			{
				return 0;
			}
		}

		// init base
		handle->swapChain = swapChain;
		handle->depthStencil = depthStencil;
		return Orbital_Video_D3D12_RenderPass_Init_Base(handle, desc, NULL, stencilUsage, true);
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(RenderPass* handle, RenderPassDesc* desc, Texture** renderTextures, RenderTextureUsage* usages, UINT renderTextureCount, DepthStencil* depthStencil, StencilUsage stencilUsage)
	{
		handle->renderTargetCount = renderTextureCount;

		// copy render-target formats
		handle->renderTargetFormats = (DXGI_FORMAT*)calloc(renderTextureCount, sizeof(DXGI_FORMAT));
		for (UINT i = 0; i != renderTextureCount; ++i)
		{
			handle->renderTargetFormats[i] = renderTextures[i]->format;
		}

		// create nodes
		handle->nodes = (RenderPassNode*)calloc(handle->device->nodeCount, sizeof(RenderPassNode));
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			handle->nodes[n].renderTargetResources = (ID3D12Resource**)calloc(renderTextureCount, sizeof(ID3D12Resource));
			handle->nodes[n].renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(renderTextureCount, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
			for (UINT i = 0; i != renderTextureCount; ++i)
			{
				handle->nodes[n].renderTargetResources[i] = renderTextures[i]->nodes[n].resource;
				handle->nodes[n].renderTargetDescs[i].cpuDescriptor = renderTextures[i]->nodes[n].renderTargetResourceDescCPUHandle;
			}
		}

		// copy render-texture references
		handle->renderTextures = (Texture**)calloc(renderTextureCount, sizeof(Texture*));
		memcpy(handle->renderTextures, renderTextures, sizeof(Texture*) * renderTextureCount);

		// init base
		handle->depthStencil = depthStencil;
		return Orbital_Video_D3D12_RenderPass_Init_Base(handle, desc, usages, stencilUsage, false);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_RenderPass_Dispose(RenderPass* handle)
	{
		if (handle->renderTextures != NULL)
		{
			free(handle->renderTextures);
			handle->renderTextures = NULL;
		}

		if (handle->renderTargetFormats != NULL)
		{
			free(handle->renderTargetFormats);
			handle->renderTargetFormats = NULL;
		}

		if (handle->nodes != NULL)
		{
			for (UINT n = 0; n != handle->device->nodeCount; ++n)
			{
				if (handle->nodes[n].renderTargetResources != NULL)
				{
					free(handle->nodes[n].renderTargetResources);
					handle->nodes[n].renderTargetResources = NULL;
				}

				if (handle->nodes[n].renderTargetDescs != NULL)
				{
					free(handle->nodes[n].renderTargetDescs);
					handle->nodes[n].renderTargetDescs = NULL;
				}

				if (handle->nodes[n].depthStencilDesc != NULL)
				{
					free(handle->nodes[n].depthStencilDesc);
					handle->nodes[n].depthStencilDesc = NULL;
				}
			}

			free(handle->nodes);
			handle->nodes = NULL;
		}

		free(handle);
	}
}