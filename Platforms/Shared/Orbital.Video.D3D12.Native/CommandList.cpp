#include "CommandList.h"
#include "SwapChain.h"
#include "RenderPass.h"
#include "RenderState.h"
#include "VertexBuffer.h"
#include "ShaderEffect.h"

extern "C"
{
	ORBITAL_EXPORT CommandList* Orbital_Video_D3D12_CommandList_Create(Device* device)
	{
		CommandList* handle = (CommandList*)calloc(1, sizeof(CommandList));
		handle->device = device;
		return handle;
	}

	ORBITAL_EXPORT int Orbital_Video_D3D12_CommandList_Init(CommandList* handle)
	{
		if (FAILED(handle->device->device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, handle->device->commandAllocator, nullptr, IID_PPV_ARGS(&handle->commandList)))) return 0;
		if (FAILED(handle->commandList->Close())) return 0;// make sure this is closed as it defaults to open for writing

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Dispose(CommandList* handle)
	{
		if (handle->pipelineState != NULL)
		{
			handle->pipelineState->Release();
			handle->pipelineState = NULL;
		}

		if (handle->commandList != NULL)
		{
			handle->commandList->Release();
			handle->commandList = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Start(CommandList* handle, Device* device)
	{
		handle->commandList->Reset(device->commandAllocator, handle->pipelineState);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Finish(CommandList* handle)
	{
		handle->commandList->Close();
	}
	
	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_BeginRenderPass(CommandList* handle, RenderPass* renderPass)
	{
		if (renderPass->swapChain != NULL)
		{
			D3D12_RESOURCE_BARRIER barrier = {};
			barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
			barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
			barrier.Transition.pResource = renderPass->swapChain->renderTargetViews[renderPass->swapChain->currentRenderTargetIndex];
			barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->commandList->ResourceBarrier(1, &barrier);
			handle->commandList->BeginRenderPass(1, &renderPass->renderTargetDescs[renderPass->swapChain->currentRenderTargetIndex], renderPass->depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
		else
		{
			D3D12_RESOURCE_BARRIER barrier = {};
			barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
			barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
			barrier.Transition.pResource = renderPass->swapChain->renderTargetViews[0];
			barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->commandList->ResourceBarrier(1, &barrier);
			handle->commandList->BeginRenderPass(1, &renderPass->renderTargetDescs[0], renderPass->depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_EndRenderPass(CommandList* handle, RenderPass* renderPass)
	{
		handle->commandList->EndRenderPass();
		if (renderPass->swapChain != NULL)
		{
			D3D12_RESOURCE_BARRIER barrier = {};
			barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
			barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
			barrier.Transition.pResource = renderPass->swapChain->renderTargetViews[renderPass->swapChain->currentRenderTargetIndex];
			barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->commandList->ResourceBarrier(1, &barrier);
		}
		else
		{
			D3D12_RESOURCE_BARRIER barrier = {};
			barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
			barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
			barrier.Transition.pResource = renderPass->swapChain->renderTargetViews[0];
			barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->commandList->ResourceBarrier(1, &barrier);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(CommandList* handle, SwapChain* swapChain, float r, float g, float b, float a)
	{
		float rgba[4] = {r, g, b, a};
		handle->commandList->ClearRenderTargetView(swapChain->renderTargetDescHandles[swapChain->currentRenderTargetIndex], rgba, 0, NULL);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_SetViewPort(CommandList* handle, UINT x, UINT y, UINT width, UINT height, float minDepth, float maxDepth)
	{
		D3D12_VIEWPORT viewPort;
		viewPort.TopLeftX = x;
		viewPort.TopLeftY = y;
		viewPort.Width = width;
		viewPort.Height = height;
		viewPort.MinDepth = minDepth;
		viewPort.MaxDepth = maxDepth;
		handle->commandList->RSSetViewports(1, &viewPort);

		D3D12_RECT rect;
		rect.left = x;
		rect.top = y;
		rect.right = x + width;
		rect.bottom = y + height;
		handle->commandList->RSSetScissorRects(1, &rect);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_SetRenderState(CommandList* handle, RenderState* renderState)
	{
		handle->commandList->SetGraphicsRootSignature(renderState->shaderEffectSignature);
		handle->commandList->IASetPrimitiveTopology(renderState->topology);
		handle->commandList->SetPipelineState(renderState->state);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_SetVertexBuffer(CommandList* handle, VertexBuffer* vertexBuffer)
	{
		handle->commandList->IASetVertexBuffers(0, 1, &vertexBuffer->vertexBufferView);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawInstanced(CommandList* handle, UINT vertexIndex, UINT vertexCount, UINT instanceCount)
	{
		handle->commandList->DrawInstanced(vertexCount, instanceCount, vertexIndex, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Execute(CommandList* handle)
	{
		ID3D12CommandList* commandLists[1] = { handle->commandList };
		handle->device->commandQueue->ExecuteCommandLists(1, commandLists);
	}
}