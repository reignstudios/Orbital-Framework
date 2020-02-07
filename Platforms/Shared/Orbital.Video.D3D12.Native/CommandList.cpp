#include "CommandList.h"
#include "SwapChain.h"
#include "RenderPass.h"
#include "RenderState.h"
#include "VertexBuffer.h"
#include "IndexBuffer.h"
#include "ShaderEffect.h"
#include "ConstantBuffer.h"

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
		// create command list
		if (FAILED(handle->device->device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, handle->device->commandAllocator, nullptr, IID_PPV_ARGS(&handle->commandList)))) return 0;
		if (FAILED(handle->commandList->Close())) return 0;// make sure this is closed as it defaults to open for writing

		// create fence
		if (FAILED(handle->device->device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&handle->fence)))) return 0;
		handle->fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
		if (handle->fenceEvent == NULL) return 0;

		// make sure fence values start at 1 so they don't match 'GetCompletedValue' when its first called
		handle->fenceValue = 1;

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Dispose(CommandList* handle)
	{
		if (handle->commandList != NULL)
		{
			handle->commandList->Release();
			handle->commandList = NULL;
		}

		if (handle->fenceEvent != NULL)
		{
			CloseHandle(handle->fenceEvent);
			handle->fenceEvent = NULL;
		}

		if (handle->fence != NULL)
		{
			handle->fence->Release();
			handle->fence = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Start(CommandList* handle, Device* device)
	{
		handle->commandList->Reset(device->commandAllocator, NULL);
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
			barrier.Transition.pResource = renderPass->renderTargetViews[renderPass->swapChain->currentRenderTargetIndex];
			barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->commandList->ResourceBarrier(1, &barrier);
			handle->commandList->BeginRenderPass(1, &renderPass->renderTargetDescs[renderPass->swapChain->currentRenderTargetIndex], renderPass->depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
		else
		{
			D3D12_RESOURCE_BARRIER barriers[D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT] = {};
			for (UINT i = 0; i != renderPass->renderTargetCount; ++i)
			{
				barriers[i].Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barriers[i].Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barriers[i].Transition.pResource = renderPass->renderTargetViews[i];
				barriers[i].Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
				barriers[i].Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
				barriers[i].Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			}
			handle->commandList->ResourceBarrier(renderPass->renderTargetCount, barriers);
			handle->commandList->BeginRenderPass(renderPass->renderTargetCount, renderPass->renderTargetDescs, renderPass->depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
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
			barrier.Transition.pResource = renderPass->renderTargetViews[renderPass->swapChain->currentRenderTargetIndex];
			barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
			barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
			barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			handle->commandList->ResourceBarrier(1, &barrier);
		}
		else
		{
			D3D12_RESOURCE_BARRIER barriers[D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT] = {};
			for (UINT i = 0; i != renderPass->renderTargetCount; ++i)
			{
				barriers[i].Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barriers[i].Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barriers[i].Transition.pResource = renderPass->renderTargetViews[i];
				barriers[i].Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
				barriers[i].Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
				barriers[i].Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
			}
			handle->commandList->ResourceBarrier(renderPass->renderTargetCount, barriers);
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
		// set constant buffer states
		for (UINT i = 0; i != renderState->constantBufferCount; ++i)
		{
			ConstantBuffer* constantBuffer = renderState->constantBuffers[i];
			Orbital_Video_D3D12_ConstantBuffer_ChangeState(constantBuffer, D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER, handle->commandList);
		}

		// set texture states
		for (UINT i = 0; i != renderState->textureCount; ++i)
		{
			Texture* texture = renderState->textures[i];
			D3D12_RESOURCE_STATES state = {};
			if (renderState->shaderEffect->textures[i].usage == ShaderEffectResourceUsage_PS)
			{
				state = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			else
			{
				state = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
				if ((renderState->shaderEffect->textures[i].usage | ShaderEffectResourceUsage_PS) != 0) state |= D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			Orbital_Video_D3D12_Texture_ChangeState(texture, state, handle->commandList);
		}

		// set vertex buffer states
		UINT vertexBufferCount = renderState->vertexBufferStreamer->vertexBufferCount;
		VertexBuffer** vertexBuffers = renderState->vertexBufferStreamer->vertexBuffers;
		for (UINT i = 0; i != vertexBufferCount; ++i) Orbital_Video_D3D12_VertexBuffer_ChangeState(vertexBuffers[i], D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER, handle->commandList);

		// set index buffer states
		IndexBuffer* indexBuffer = renderState->indexBuffer;
		if (indexBuffer != NULL) Orbital_Video_D3D12_IndexBuffer_ChangeState(indexBuffer, D3D12_RESOURCE_STATE_INDEX_BUFFER, handle->commandList);

		// bind shader resources
		handle->commandList->SetGraphicsRootSignature(renderState->shaderEffect->signatures[0]);// TODO: handle multi-gpu

		UINT descIndex = 0;
		if (renderState->constantBufferHeap != NULL)
		{
			handle->commandList->SetDescriptorHeaps(1, &renderState->constantBufferHeap);
			handle->commandList->SetGraphicsRootDescriptorTable(descIndex, renderState->constantBufferGPUDescHandle);
			++descIndex;
		}

		if (renderState->textureHeap != NULL)
		{
			handle->commandList->SetDescriptorHeaps(1, &renderState->textureHeap);
			handle->commandList->SetGraphicsRootDescriptorTable(descIndex, renderState->textureGPUDescHandle);
		}

		// enable render state
		handle->commandList->SetPipelineState(renderState->state);

		// enable vertex / index buffers
		handle->commandList->IASetPrimitiveTopology(renderState->topology);
		handle->commandList->IASetVertexBuffers(0, vertexBufferCount, renderState->vertexBufferStreamer->vertexBufferViews);
		if (indexBuffer != NULL) handle->commandList->IASetIndexBuffer(&indexBuffer->indexBufferView);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawInstanced(CommandList* handle, UINT vertexOffset, UINT vertexCount, UINT instanceCount)
	{
		handle->commandList->DrawInstanced(vertexCount, instanceCount, vertexOffset, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(CommandList* handle, UINT vertexOffset, UINT indexOffset, UINT indexCount, UINT instanceCount)
	{
		handle->commandList->DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Execute(CommandList* handle)
	{
		ID3D12CommandList* commandLists[1] = { handle->commandList };
		handle->device->commandQueue->ExecuteCommandLists(1, commandLists);
		WaitForFence(handle->device, handle->fence, handle->fenceEvent, handle->fenceValue);// make sure gpu has finished before we continue
	}
}