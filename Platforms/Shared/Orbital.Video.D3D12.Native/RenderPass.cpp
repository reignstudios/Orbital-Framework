#include "RenderPass.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT RenderPass* Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(Device* device, SwapChain* swapChain, DepthStencil* depthStencil)
	{
		RenderPass* handle = (RenderPass*)calloc(1, sizeof(RenderPass));
		handle->device = device;
		handle->swapChain = swapChain;
		handle->depthStencil = depthStencil;

		// allocate swap-chain specific resources
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

		return handle;
	}

	ORBITAL_EXPORT RenderPass* Orbital_Video_D3D12_RenderPass_Create_WithRenderTextures(Device* device, Texture** renderTextures, UINT renderTextureCount, DepthStencil* depthStencil)
	{
		RenderPass* handle = (RenderPass*)calloc(1, sizeof(RenderPass));
		handle->device = device;
		handle->depthStencil = depthStencil;

		// allocate swap-chain specific resources
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

		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init(RenderPass* handle, RenderPassDesc* desc)
	{
		// render-pass: render target
		for (UINT i = 0; i != handle->renderTargetCount; ++i)
		{
			// begin
			handle->renderTargetDescs[i].BeginningAccess.Type = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
			memcpy(handle->renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Color, desc->clearColorValue, sizeof(float) * 4);
			handle->renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Format = handle->renderTargetFormats[i];

			// end
			handle->renderTargetDescs[i].EndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
			//handle->renderTargetDescs[i].EndingAccess.Resolve// TODO: if MSAA: D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_RESOLVE
		}

		// render-pass: depth stencil
		if (handle->depthStencil != NULL)
		{
			handle->depthStencilFormat = handle->depthStencil->format;

			handle->depthStencilDesc = (D3D12_RENDER_PASS_DEPTH_STENCIL_DESC*)calloc(1, sizeof(D3D12_RENDER_PASS_DEPTH_STENCIL_DESC));
			handle->depthStencilDesc->cpuDescriptor = handle->depthStencil->resourceCPUHeapHandle;

			// begin depth
			handle->depthStencilDesc->DepthBeginningAccess.Type = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
			handle->depthStencilDesc->DepthBeginningAccess.Clear.ClearValue.DepthStencil.Depth = desc->depthValue;
			handle->depthStencilDesc->DepthBeginningAccess.Clear.ClearValue.Format = handle->depthStencil->format;

			// end depth
			handle->depthStencilDesc->DepthEndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
			//handle->depthStencilDesc->DepthEndingAccess.Resolve// ??

			// begin stencil
			handle->depthStencilDesc->StencilBeginningAccess.Type = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
			handle->depthStencilDesc->StencilBeginningAccess.Clear.ClearValue.DepthStencil.Stencil = desc->stencilValue;
			handle->depthStencilDesc->StencilBeginningAccess.Clear.ClearValue.Format = handle->depthStencil->format;

			// end stencil
			handle->depthStencilDesc->StencilEndingAccess.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;
			//handle->depthStencilDesc->StencilEndingAccess.Resolve// ??
		}

		return 1;
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