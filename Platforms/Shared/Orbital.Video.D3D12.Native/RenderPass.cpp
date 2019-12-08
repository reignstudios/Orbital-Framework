#include "RenderPass.h"
#include "Utils.h"

extern "C"
{
	ORBITAL_EXPORT RenderPass* Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(Device* device, SwapChain* swapChain)
	{
		RenderPass* handle = (RenderPass*)calloc(1, sizeof(RenderPass));
		handle->device = device;
		handle->swapChain = swapChain;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init_Native(RenderPass* handle, RenderPassDesc* desc, DXGI_FORMAT* renderTargetFormats, D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetHandles, UINT renderTargetCount)//, DXGI_FORMAT depthStencilFormat)
	{
		// render-pass: render target
		handle->renderTargetCount = renderTargetCount;
		handle->renderTargetDescs = (D3D12_RENDER_PASS_RENDER_TARGET_DESC*)calloc(renderTargetCount, sizeof(D3D12_RENDER_PASS_RENDER_TARGET_DESC));
		handle->renderTargetFormats = (DXGI_FORMAT*)calloc(renderTargetCount, sizeof(DXGI_FORMAT));
		for (UINT i = 0; i != renderTargetCount; ++i)
		{
			handle->renderTargetFormats[i] = renderTargetFormats[i];

			D3D12_RENDER_PASS_BEGINNING_ACCESS renderPassBeginningAccessClear;
			renderPassBeginningAccessClear.Type = D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE::D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_CLEAR;
			memcpy(renderPassBeginningAccessClear.Clear.ClearValue.Color, desc->clearColorValue, sizeof(float) * 4);
			//renderPassBeginningAccessClear.Clear.ClearValue.DepthStencil.Depth = desc->depthValue;
			//renderPassBeginningAccessClear.Clear.ClearValue.DepthStencil.Stencil = desc->stencilValue;

			D3D12_RENDER_PASS_ENDING_ACCESS renderPassEndingAccessPreserve;
			renderPassEndingAccessPreserve.Type = D3D12_RENDER_PASS_ENDING_ACCESS_TYPE::D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_PRESERVE;

			handle->renderTargetDescs[i].cpuDescriptor = renderTargetHandles[i];
			handle->renderTargetDescs[i].BeginningAccess = renderPassBeginningAccessClear;
			handle->renderTargetDescs[i].EndingAccess = renderPassEndingAccessPreserve;
		}

		// render-pass: depth stencil
		//handle->depthStencilFormat = depthStencilFormat;
		//D3D12_RENDER_PASS_BEGINNING_ACCESS renderPassBeginningAccessNoAccess{ D3D12_RENDER_PASS_BEGINNING_ACCESS_TYPE_NO_ACCESS, {} };
		//D3D12_RENDER_PASS_ENDING_ACCESS renderPassEndingAccessNoAccess{ D3D12_RENDER_PASS_ENDING_ACCESS_TYPE_NO_ACCESS, {} };
		//D3D12_RENDER_PASS_DEPTH_STENCIL_DESC renderPassDepthStencilDesc{ dsvCPUDescriptorHandle, renderPassBeginningAccessNoAccess, renderPassBeginningAccessNoAccess, renderPassEndingAccessNoAccess, renderPassEndingAccessNoAccess };

		return 1;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_RenderPass_Init(RenderPass* handle, RenderPassDesc* desc)
	{
		if (handle->swapChain != NULL)
		{
			DXGI_FORMAT* renderTargetFormatFormats = (DXGI_FORMAT*)alloca(sizeof(DXGI_FORMAT) * handle->swapChain->bufferCount);
			for (UINT i = 0; i != handle->swapChain->bufferCount; ++i) renderTargetFormatFormats[i] = handle->swapChain->renderTargetFormat;
			return Orbital_Video_D3D12_RenderPass_Init_Native(handle, desc, renderTargetFormatFormats, handle->swapChain->renderTargetDescHandles, handle->swapChain->bufferCount);//, handle->depthStencilFormat);
		}
		else
		{
			return 0;//Orbital_Video_D3D12_RenderPass_Init_Native(handle, desc, &handle->texture->format, &handle->texture->renderTargetDescHandle, 1);//, handle->depthStencil->format);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_RenderPass_Dispose(RenderPass* handle)
	{
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

		if (handle->renderTargetViews != NULL)
		{
			free(handle->renderTargetViews);
			handle->renderTargetViews = NULL;
		}

		free(handle);
	}
}