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
			handle->renderTargetDescs[i].BeginningAccess.Type = accessType;
			memcpy(handle->renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Color, desc->renderTargetDescs[d].clearColorValue, sizeof(float) * 4);
			handle->renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Format = handle->renderTargetFormats[i];

			// end
			handle->renderTargetDescs[i].EndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
			//handle->renderTargetDescs[i].EndingAccess.Resolve// TODO: if MSAA: D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_RESOLVE

			if (!onlyUseFirstRenderTargetDesc) ++d;
		}

		// render-pass: depth stencil
		if (handle->depthStencil != NULL)
		{
			handle->depthStencilFormat = handle->depthStencil->format;

			handle->depthStencilDesc = (D3D12_RENDER_PASS_DEPTH_STENCIL_DESC*)calloc(1, sizeof(D3D12_RENDER_PASS_DEPTH_STENCIL_DESC));
			handle->depthStencilDesc->cpuDescriptor = handle->depthStencil->resourceCPUHeapHandle;

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
			handle->depthStencilDesc->DepthBeginningAccess.Type = depthAccessType;
			handle->depthStencilDesc->DepthBeginningAccess.Clear.ClearValue.DepthStencil.Depth = desc->depthStencilDesc.depthValue;
			handle->depthStencilDesc->DepthBeginningAccess.Clear.ClearValue.Format = handle->depthStencil->format;

			// end depth
			handle->depthStencilDesc->DepthEndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
			//handle->depthStencilDesc->DepthEndingAccess.Resolve// ??

			// begin stencil
			handle->depthStencilDesc->StencilBeginningAccess.Type = stencilAccessType;
			handle->depthStencilDesc->StencilBeginningAccess.Clear.ClearValue.DepthStencil.Stencil = desc->depthStencilDesc.stencilValue;
			handle->depthStencilDesc->StencilBeginningAccess.Clear.ClearValue.Format = handle->depthStencil->format;

			// end stencil
			handle->depthStencilDesc->StencilEndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
			//handle->depthStencilDesc->StencilEndingAccess.Resolve// ??
		}

		return 1;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init_WithSwapChain(RenderPass* handle, RenderPassDesc* desc, SwapChain* swapChain, DepthStencil* depthStencil, StencilUsage stencilUsage)
	{
		handle->renderTargetCount = swapChain->bufferCount;
		handle->renderTargetFormats = (DXGI_FORMAT*)calloc(swapChain->bufferCount, sizeof(DXGI_FORMAT));
		handle->renderTargetResources = (ID3D12Resource**)calloc(swapChain->bufferCount, sizeof(ID3D12Resource));
		handle->renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(swapChain->bufferCount, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
		for (UINT i = 0; i != swapChain->bufferCount; ++i)
		{
			handle->renderTargetFormats[i] = swapChain->format;
			handle->renderTargetResources[i] = swapChain->resources[i];
			handle->renderTargetDescs[i].cpuDescriptor = swapChain->resourceDescCPUHandles[i];
		}

		handle->swapChain = swapChain;
		handle->depthStencil = depthStencil;
		return Orbital_Video_D3D12_RenderPass_Init_Base(handle, desc, NULL, stencilUsage, true);
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(RenderPass* handle, RenderPassDesc* desc, Texture** renderTextures, RenderTextureUsage* usages, UINT renderTextureCount, DepthStencil* depthStencil, StencilUsage stencilUsage)
	{
		handle->renderTargetCount = renderTextureCount;
		handle->renderTargetFormats = (DXGI_FORMAT*)calloc(renderTextureCount, sizeof(DXGI_FORMAT));
		handle->renderTargetResources = (ID3D12Resource**)calloc(renderTextureCount, sizeof(ID3D12Resource));
		handle->renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(renderTextureCount, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
		for (UINT i = 0; i != renderTextureCount; ++i)
		{
			handle->renderTargetFormats[i] = renderTextures[i]->format;
			handle->renderTargetResources[i] = renderTextures[i]->resource;
			handle->renderTargetDescs[i].cpuDescriptor = renderTextures[i]->renderTargetResourceDescCPUHandle;
		}

		handle->depthStencil = depthStencil;
		return Orbital_Video_D3D12_RenderPass_Init_Base(handle, desc, usages, stencilUsage, false);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_RenderPass_Dispose(RenderPass* handle)
	{
		if (handle->renderTargetFormats != NULL)
		{
			free(handle->renderTargetFormats);
			handle->renderTargetFormats = NULL;
		}

		if (handle->renderTargetResources != NULL)
		{
			free(handle->renderTargetResources);
			handle->renderTargetResources = NULL;
		}

		if (handle->renderTargetDescs != NULL)
		{
			free(handle->renderTargetDescs);
			handle->renderTargetDescs = NULL;
		}

		if (handle->depthStencilDesc != NULL)
		{
			free(handle->depthStencilDesc);
			handle->depthStencilDesc = NULL;
		}

		free(handle);
	}
}