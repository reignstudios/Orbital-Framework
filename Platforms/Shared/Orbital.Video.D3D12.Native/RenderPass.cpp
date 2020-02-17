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
		return handle;
	}

	int Orbital_Video_D3D12_RenderPass_Init_Base(RenderPass* handle, RenderPassDesc* desc, DXGI_FORMAT* renderTargetFormats, ID3D12Resource** renderTargetViews, D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetHandles, UINT renderTargetCount)
	{
		// render-pass: render target
		handle->renderTargetCount = renderTargetCount;
		handle->renderTargetFormats = (DXGI_FORMAT*)calloc(renderTargetCount, sizeof(DXGI_FORMAT));
		handle->renderTargetViews = (ID3D12Resource**)calloc(renderTargetCount, sizeof(ID3D12Resource));
		handle->renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(renderTargetCount, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
		for (UINT i = 0; i != renderTargetCount; ++i)
		{
			handle->renderTargetFormats[i] = renderTargetFormats[i];
			handle->renderTargetViews[i] = renderTargetViews[i];

			handle->renderTargetDescs[i].cpuDescriptor = renderTargetHandles[i];

			// begin
			handle->renderTargetDescs[i].BeginningAccess.Type = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
			memcpy(handle->renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Color, desc->clearColorValue, sizeof(float) * 4);
			handle->renderTargetDescs[i].BeginningAccess.Clear.ClearValue.Format = renderTargetFormats[i];

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

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init(RenderPass* handle, RenderPassDesc* desc)
	{
		if (handle->swapChain != NULL)
		{
			DXGI_FORMAT* renderTargetFormatFormats = (DXGI_FORMAT*)alloca(sizeof(DXGI_FORMAT) * handle->swapChain->bufferCount);
			for (UINT i = 0; i != handle->swapChain->bufferCount; ++i) renderTargetFormatFormats[i] = handle->swapChain->renderTargetFormat;
			return Orbital_Video_D3D12_RenderPass_Init_Base(handle, desc, renderTargetFormatFormats, handle->swapChain->renderTargetViews, handle->swapChain->renderTargetDescCPUHandles, handle->swapChain->bufferCount);
		}
		else
		{
			return 0;//Orbital_Video_D3D12_RenderPass_Init_Base(handle, desc, &handle->texture->format, &handle->texture->renderTargetDescHandle, 1);//, handle->depthStencil->format);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_RenderPass_Dispose(RenderPass* handle)
	{
		if (handle->renderTargetFormats != NULL)
		{
			free(handle->renderTargetFormats);
			handle->renderTargetFormats = NULL;
		}

		if (handle->renderTargetViews != NULL)
		{
			free(handle->renderTargetViews);
			handle->renderTargetViews = NULL;
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