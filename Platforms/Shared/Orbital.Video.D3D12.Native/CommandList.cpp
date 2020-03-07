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
			D3D12_RESOURCE_BARRIER barriers[2] = {};
			int barrierCount = 0;
			if (renderPass->swapChain->resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET)
			{
				auto barrier = &barriers[barrierCount];
				barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barrier->Transition.pResource = renderPass->renderTargetResources[renderPass->swapChain->currentRenderTargetIndex];
				barrier->Transition.StateBefore = renderPass->swapChain->resourceState;
				barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
				barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				renderPass->swapChain->resourceState = barrier->Transition.StateAfter;
				++barrierCount;
			}

			if (renderPass->depthStencil != NULL && renderPass->depthStencil->resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE)
			{
				auto barrier = &barriers[barrierCount];
				barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barrier->Transition.pResource = renderPass->depthStencil->resource;
				barrier->Transition.StateBefore = renderPass->depthStencil->resourceState;
				barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE;
				barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				renderPass->depthStencil->resourceState = barrier->Transition.StateAfter;
				++barrierCount;
			}
			
			if (barrierCount != 0) handle->commandList->ResourceBarrier(barrierCount, barriers);
			handle->commandList->BeginRenderPass(1, &renderPass->renderTargetDescs[renderPass->swapChain->currentRenderTargetIndex], renderPass->depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
		else
		{
			D3D12_RESOURCE_BARRIER barriers[D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT + 1] = {};
			UINT barrierCount = 0;
			for (UINT i = 0; i != renderPass->renderTargetCount; ++i)
			{
				auto renderTexture = renderPass->renderTextures[barrierCount];
				if (renderTexture->resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET)
				{
					auto barrier = &barriers[barrierCount];
					barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
					barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
					barrier->Transition.pResource = renderPass->renderTargetResources[barrierCount];
					barrier->Transition.StateBefore = renderTexture->resourceState;
					barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
					barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
					renderTexture->resourceState = barrier->Transition.StateAfter;
					++barrierCount;
				}
			}

			if (renderPass->depthStencil != NULL && renderPass->depthStencil->resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE)
			{
				auto barrier = &barriers[barrierCount];
				barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barrier->Transition.pResource = renderPass->depthStencil->resource;
				barrier->Transition.StateBefore = renderPass->depthStencil->resourceState;
				barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE;
				barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				renderPass->depthStencil->resourceState = barrier->Transition.StateAfter;
				++barrierCount;
			}
			
			if (barrierCount != 0) handle->commandList->ResourceBarrier(barrierCount, barriers);
			handle->commandList->BeginRenderPass(renderPass->renderTargetCount, renderPass->renderTargetDescs, renderPass->depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_EndRenderPass(CommandList* handle, RenderPass* renderPass)
	{
		handle->commandList->EndRenderPass();
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(CommandList* handle, SwapChain* swapChain, float r, float g, float b, float a)
	{
		float rgba[4] = {r, g, b, a};
		handle->commandList->ClearRenderTargetView(swapChain->resourceDescCPUHandles[swapChain->currentRenderTargetIndex], rgba, 0, NULL);
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
		UINT t = 0;
		for (UINT i = 0; i != renderState->textureCount; ++i)
		{
			Texture* texture = renderState->textures[i];
			D3D12_RESOURCE_STATES state = {};
			if (renderState->shaderEffect->textures[t].usage == ShaderEffectResourceUsage_PS)
			{
				state = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			else
			{
				state = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
				if ((renderState->shaderEffect->textures[t].usage | ShaderEffectResourceUsage_PS) != 0) state |= D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			Orbital_Video_D3D12_Texture_ChangeState(texture, state, handle->commandList);
			++t;
		}

		for (UINT i = 0; i != renderState->textureDepthStencilCount; ++i)
		{
			DepthStencil* depthStencil = renderState->textureDepthStencils[i];
			D3D12_RESOURCE_STATES state = {};
			if (renderState->shaderEffect->textures[t].usage == ShaderEffectResourceUsage_PS)
			{
				state = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			else
			{
				state = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
				if ((renderState->shaderEffect->textures[t].usage | ShaderEffectResourceUsage_PS) != 0) state |= D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			Orbital_Video_D3D12_DepthStencil_ChangeState(depthStencil, state, handle->commandList);
			++t;
		}

		// set vertex buffer states
		UINT vertexBufferCount = renderState->vertexBufferStreamer->vertexBufferCount;
		VertexBuffer** vertexBuffers = renderState->vertexBufferStreamer->vertexBuffers;
		for (UINT i = 0; i != vertexBufferCount; ++i) Orbital_Video_D3D12_VertexBuffer_ChangeState(vertexBuffers[i], D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER, handle->commandList);

		// set index buffer states
		IndexBuffer* indexBuffer = renderState->indexBuffer;
		if (indexBuffer != NULL) Orbital_Video_D3D12_IndexBuffer_ChangeState(indexBuffer, D3D12_RESOURCE_STATE_INDEX_BUFFER, handle->commandList);

		// bind shader resources
		handle->commandList->SetGraphicsRootSignature(renderState->shaderEffect->signature);

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
		if (indexBuffer != NULL) handle->commandList->IASetIndexBuffer(&indexBuffer->resourceView);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawInstanced(CommandList* handle, UINT vertexOffset, UINT vertexCount, UINT instanceCount)
	{
		handle->commandList->DrawInstanced(vertexCount, instanceCount, vertexOffset, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(CommandList* handle, UINT vertexOffset, UINT indexOffset, UINT indexCount, UINT instanceCount)
	{
		handle->commandList->DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_ResolveRenderTexture(CommandList* handle, Texture* srcRenderTexture, Texture* dstRenderTexture)
	{
		Orbital_Video_D3D12_Texture_ChangeState(srcRenderTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_SOURCE, handle->commandList);
		Orbital_Video_D3D12_Texture_ChangeState(dstRenderTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_DEST, handle->commandList);
		handle->commandList->ResolveSubresource(dstRenderTexture->resource, 0, srcRenderTexture->resource, 0, srcRenderTexture->format);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_ResolveRenderTextureToSwapChain(CommandList* handle, Texture* srcRenderTexture, SwapChain* dstSwapChain)
	{
		Orbital_Video_D3D12_Texture_ChangeState(srcRenderTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_SOURCE, handle->commandList);
		Orbital_Video_D3D12_SwapChain_ChangeState(dstSwapChain, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_DEST, handle->commandList);
		handle->commandList->ResolveSubresource(dstSwapChain->resources[dstSwapChain->currentRenderTargetIndex], 0, srcRenderTexture->resource, 0, srcRenderTexture->format);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_CopyTexture(CommandList* handle, Texture* srcTexture, Texture* dstTexture)
	{
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->commandList);
		Orbital_Video_D3D12_Texture_ChangeState(dstTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->commandList);
		handle->commandList->CopyResource(dstTexture->resource, srcTexture->resource);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_CopyTextureToSwapChain(CommandList* handle, Texture* srcTexture, SwapChain* dstSwapChain)
	{
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->commandList);
		Orbital_Video_D3D12_SwapChain_ChangeState(dstSwapChain, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->commandList);
		handle->commandList->CopyResource(dstSwapChain->resources[dstSwapChain->currentRenderTargetIndex], srcTexture->resource);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_CopyTextureRegion(CommandList* handle, Texture* srcTexture, Texture* dstTexture, UINT srcX, UINT srcY, UINT srcZ, UINT dstX, UINT dstY, UINT dstZ, UINT width, UINT height, UINT depth, UINT srcMipmapLevel, UINT dstMipmapLevel)
	{
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->commandList);
		Orbital_Video_D3D12_Texture_ChangeState(dstTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->commandList);

		D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
		dstLoc.pResource = dstTexture->resource;
		dstLoc.SubresourceIndex = dstMipmapLevel;

		D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
		srcLoc.pResource = srcTexture->resource;
		srcLoc.SubresourceIndex = srcMipmapLevel;

		D3D12_BOX srcBox;
		srcBox.left = srcX;
		srcBox.right = srcX + width;
		srcBox.top = srcY;
		srcBox.bottom = srcY + height;
		srcBox.front = srcZ;
		srcBox.back = srcZ + depth;

		handle->commandList->CopyTextureRegion(&dstLoc, dstX, dstY, dstZ, &srcLoc, &srcBox);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_CopyTextureToSwapChainRegion(CommandList* handle, Texture* srcTexture, SwapChain* dstSwapChain, UINT srcX, UINT srcY, UINT srcZ, UINT dstX, UINT dstY, UINT dstZ, UINT width, UINT height, UINT depth, UINT srcMipmapLevel)
	{
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->commandList);
		Orbital_Video_D3D12_SwapChain_ChangeState(dstSwapChain, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->commandList);

		D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
		dstLoc.pResource = dstSwapChain->resources[dstSwapChain->currentRenderTargetIndex];

		D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
		srcLoc.pResource = srcTexture->resource;
		srcLoc.SubresourceIndex = srcMipmapLevel;

		D3D12_BOX srcBox;
		srcBox.left = srcX;
		srcBox.right = srcX + width;
		srcBox.top = srcY;
		srcBox.bottom = srcY + height;
		srcBox.front = srcZ;
		srcBox.back = srcZ + depth;

		handle->commandList->CopyTextureRegion(&dstLoc, dstX, dstY, dstZ, &srcLoc, &srcBox);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Execute(CommandList* handle)
	{
		ID3D12CommandList* commandLists[1] = { handle->commandList };
		handle->device->commandQueue->ExecuteCommandLists(1, commandLists);
		WaitForFence(handle->device, handle->fence, handle->fenceEvent, handle->fenceValue);// make sure gpu has finished before we continue
	}
}