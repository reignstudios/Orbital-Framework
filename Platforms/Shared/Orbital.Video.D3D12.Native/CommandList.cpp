#include "CommandList.h"
#include "SwapChain.h"
#include "RenderPass.h"
#include "RenderState.h"
#include "ComputeState.h"
#include "ComputeShader.h"
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

	ORBITAL_EXPORT int Orbital_Video_D3D12_CommandList_Init(CommandList* handle, CommandListType type)
	{
		// create nodes
		handle->nodes = (CommandListNode*)calloc(handle->device->nodeCount, sizeof(CommandListNode));
		for (UINT n = 0; n != handle->device->nodeCount; ++n)
		{
			UINT nodeMask = handle->device->nodes[n].mask;

			// prep and reference
			D3D12_COMMAND_LIST_TYPE nativeType;
			handle->nodes[n].commandQueueRef_Direct = handle->device->nodes[n].commandQueue;
			if (type == CommandListType::CommandListType_Rasterize)
			{
				nativeType = D3D12_COMMAND_LIST_TYPE_DIRECT;
				handle->nodes[n].commandQueueRef = handle->device->nodes[n].commandQueue;
				if (FAILED(handle->device->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&handle->nodes[n].commandAllocator)))) return 0;
			}
			else if (type == CommandListType::CommandListType_Compute)
			{
				nativeType = D3D12_COMMAND_LIST_TYPE_COMPUTE;
				handle->nodes[n].commandQueueRef = handle->device->nodes[n].commandQueue_Compute;
				if (FAILED(handle->device->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_COMPUTE, IID_PPV_ARGS(&handle->nodes[n].commandAllocator)))) return 0;
				if (FAILED(handle->device->device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&handle->nodes[n].commandAllocator_Direct)))) return 0;
			}
			else
			{
				return 0;
			}

			// create command list
			if (FAILED(handle->device->device->CreateCommandList(nodeMask, nativeType, handle->nodes[n].commandAllocator, nullptr, IID_PPV_ARGS(&handle->nodes[n].commandList)))) return 0;
			if (FAILED(handle->nodes[n].commandList->Close())) return 0;// make sure this is closed as it defaults to open for writing
			if (FAILED(handle->nodes[n].commandList->QueryInterface(&handle->nodes[n].commandList4))) return 0;

			// create extra direct command list for resource transisions that compute command lists don't support if needed
			if (type == CommandListType::CommandListType_Compute)
			{
				if (FAILED(handle->device->device->CreateCommandList(nodeMask, D3D12_COMMAND_LIST_TYPE_DIRECT, handle->nodes[n].commandAllocator_Direct, nullptr, IID_PPV_ARGS(&handle->nodes[n].commandList_Direct)))) return 0;
				if (FAILED(handle->nodes[n].commandList_Direct->Close())) return 0;// make sure this is closed as it defaults to open for writing
			}
			else
			{
				handle->nodes[n].commandList_Direct = handle->nodes[n].commandList;
			}

			// create fence
			if (FAILED(handle->device->device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&handle->nodes[n].fence)))) return 0;
			handle->nodes[n].fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
			if (handle->nodes[n].fenceEvent == NULL) return 0;

			// make sure fence values start at 1 so they don't match 'GetCompletedValue' when its first called
			handle->nodes[n].fenceValue = 1;
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Dispose(CommandList* handle)
	{
		if (handle->nodes != NULL)
		{
			for (UINT n = 0; n != handle->device->nodeCount; ++n)
			{
				if (handle->nodes[n].commandList_Direct != NULL && handle->nodes[n].commandList_Direct != handle->nodes[n].commandList)
				{
					handle->nodes[n].commandList_Direct->Release();
					handle->nodes[n].commandList_Direct = NULL;
				}
				else
				{
					handle->nodes[n].commandList_Direct = NULL;
				}

				if (handle->nodes[n].commandList4 != NULL)
				{
					handle->nodes[n].commandList4->Release();
					handle->nodes[n].commandList4 = NULL;
				}

				if (handle->nodes[n].commandList != NULL)
				{
					handle->nodes[n].commandList->Release();
					handle->nodes[n].commandList = NULL;
				}

				if (handle->nodes[n].commandAllocator != NULL)
				{
					handle->nodes[n].commandAllocator->Release();
					handle->nodes[n].commandAllocator = NULL;
				}

				if (handle->nodes[n].commandAllocator_Direct != NULL)
				{
					handle->nodes[n].commandAllocator_Direct->Release();
					handle->nodes[n].commandAllocator_Direct = NULL;
				}

				if (handle->nodes[n].fenceEvent != NULL)
				{
					CloseHandle(handle->nodes[n].fenceEvent);
					handle->nodes[n].fenceEvent = NULL;
				}

				if (handle->nodes[n].fence != NULL)
				{
					handle->nodes[n].fence->Release();
					handle->nodes[n].fence = NULL;
				}
			}

			free(handle->nodes);
			handle->nodes = NULL;
		}

		free(handle);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Start(CommandList* handle, int nodeIndex)
	{
		handle->activeNodeIndex = nodeIndex;
		handle->activeNode = &handle->nodes[nodeIndex];
		handle->activeNode->commandAllocator->Reset();
		handle->activeNode->commandList->Reset(handle->activeNode->commandAllocator, NULL);
		if (handle->activeNode->commandList_Direct != handle->activeNode->commandList)
		{
			handle->activeNode->commandAllocator_Direct->Reset();
			handle->activeNode->commandList_Direct->Reset(handle->activeNode->commandAllocator_Direct, NULL);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Finish(CommandList* handle)
	{
		handle->activeNode->commandList->Close();
		if (handle->activeNode->commandList_Direct != handle->activeNode->commandList) handle->activeNode->commandList_Direct->Close();
	}
	
	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_BeginRenderPass(CommandList* handle, RenderPass* renderPass)
	{
		UINT activeNodeIndex = handle->activeNodeIndex;
		if (renderPass->swapChain != NULL)
		{
			UINT activeRenderTargetIndex = renderPass->swapChain->activeRenderTargetIndex;
			D3D12_RESOURCE_BARRIER barriers[2] = {};
			int barrierCount = 0;
			if (renderPass->swapChain->resourceStates[activeRenderTargetIndex] != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET)
			{
				auto barrier = &barriers[barrierCount];
				barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				if (renderPass->swapChain->type == SwapChainType::SwapChainType_SingleGPU_Standard)
				{
					barrier->Transition.pResource = renderPass->nodes[activeNodeIndex].renderTargetResources[activeRenderTargetIndex];
				}
				else if (renderPass->swapChain->type == SwapChainType::SwapChainType_MultiGPU_AFR)
				{
					barrier->Transition.pResource = renderPass->nodes[activeNodeIndex].renderTargetResources[0];
				}
				barrier->Transition.StateBefore = renderPass->swapChain->resourceStates[activeRenderTargetIndex];
				barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
				barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				renderPass->swapChain->resourceStates[activeRenderTargetIndex] = barrier->Transition.StateAfter;
				++barrierCount;
			}

			if (renderPass->depthStencil != NULL && renderPass->depthStencil->nodes[activeNodeIndex].resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE)
			{
				auto barrier = &barriers[barrierCount];
				barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barrier->Transition.pResource = renderPass->depthStencil->nodes[activeNodeIndex].resource;
				barrier->Transition.StateBefore = renderPass->depthStencil->nodes[activeNodeIndex].resourceState;
				barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE;
				barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				renderPass->depthStencil->nodes[activeNodeIndex].resourceState = barrier->Transition.StateAfter;
				++barrierCount;
			}
			
			if (barrierCount != 0) handle->activeNode->commandList->ResourceBarrier(barrierCount, barriers);
			D3D12_RENDER_PASS_RENDER_TARGET_DESC* renderTargetDescs = nullptr;
			if (renderPass->swapChain->type == SwapChainType::SwapChainType_SingleGPU_Standard) renderTargetDescs = &renderPass->nodes[activeNodeIndex].renderTargetDescs[activeRenderTargetIndex];
			else if (renderPass->swapChain->type == SwapChainType::SwapChainType_MultiGPU_AFR) renderTargetDescs = &renderPass->nodes[activeNodeIndex].renderTargetDescs[0];
			handle->activeNode->commandList4->BeginRenderPass(1, renderTargetDescs, renderPass->nodes[activeNodeIndex].depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
		else
		{
			D3D12_RESOURCE_BARRIER barriers[D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT + 1] = {};
			UINT barrierCount = 0;
			for (UINT i = 0; i != renderPass->renderTargetCount; ++i)
			{
				auto renderTexture = renderPass->renderTextures[barrierCount];
				if (renderTexture->nodes[activeNodeIndex].resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET)
				{
					auto barrier = &barriers[barrierCount];
					barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
					barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
					barrier->Transition.pResource = renderPass->nodes[activeNodeIndex].renderTargetResources[barrierCount];
					barrier->Transition.StateBefore = renderTexture->nodes[activeNodeIndex].resourceState;
					barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
					barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
					renderTexture->nodes[activeNodeIndex].resourceState = barrier->Transition.StateAfter;
					++barrierCount;
				}
			}

			if (renderPass->depthStencil != NULL && renderPass->depthStencil->nodes[activeNodeIndex].resourceState != D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE)
			{
				auto barrier = &barriers[barrierCount];
				barrier->Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
				barrier->Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
				barrier->Transition.pResource = renderPass->depthStencil->nodes[activeNodeIndex].resource;
				barrier->Transition.StateBefore = renderPass->depthStencil->nodes[activeNodeIndex].resourceState;
				barrier->Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_DEPTH_WRITE;
				barrier->Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
				renderPass->depthStencil->nodes[activeNodeIndex].resourceState = barrier->Transition.StateAfter;
				++barrierCount;
			}
			
			if (barrierCount != 0) handle->activeNode->commandList->ResourceBarrier(barrierCount, barriers);
			handle->activeNode->commandList4->BeginRenderPass(renderPass->renderTargetCount, renderPass->nodes[activeNodeIndex].renderTargetDescs, renderPass->nodes[activeNodeIndex].depthStencilDesc, D3D12_RENDER_PASS_FLAGS::D3D12_RENDER_PASS_FLAG_NONE);
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_EndRenderPass(CommandList* handle, RenderPass* renderPass)
	{
		handle->activeNode->commandList4->EndRenderPass();
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
		handle->activeNode->commandList->RSSetViewports(1, &viewPort);

		D3D12_RECT rect;
		rect.left = x;
		rect.top = y;
		rect.right = x + width;
		rect.bottom = y + height;
		handle->activeNode->commandList->RSSetScissorRects(1, &rect);
	}

	void UpdateStateResources
	(
		CommandList* handle,
		ShaderSignature* signature,
		UINT constantBufferCount, ConstantBuffer** constantBuffers,
		UINT textureCount, Texture** textures,
		UINT textureDepthStencilCount, DepthStencil** textureDepthStencils,
		UINT randomAccessBufferCount, intptr_t* randomAccessBuffers, RandomAccessBufferType* randomAccessTypes
	)
	{
		// set constant buffer states
		for (UINT i = 0; i != constantBufferCount; ++i)
		{
			ConstantBuffer* constantBuffer = constantBuffers[i];
			Orbital_Video_D3D12_ConstantBuffer_ChangeState(constantBuffer, handle->activeNodeIndex, D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER, handle->activeNode->commandList_Direct);
		}

		// set texture states
		UINT t = 0;
		for (UINT i = 0; i != textureCount; ++i)
		{
			Texture* texture = textures[i];
			D3D12_RESOURCE_STATES state = {};
			if (signature->textures[t].usage == ShaderEffectResourceUsage_PS)
			{
				state = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			else
			{
				state = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
				if ((signature->textures[t].usage | ShaderEffectResourceUsage_PS) != 0) state |= D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			Orbital_Video_D3D12_Texture_ChangeState(texture, handle->activeNodeIndex, state, handle->activeNode->commandList_Direct);
			++t;
		}

		for (UINT i = 0; i != textureDepthStencilCount; ++i)
		{
			DepthStencil* depthStencil = textureDepthStencils[i];
			D3D12_RESOURCE_STATES state = {};
			if (signature->textures[t].usage == ShaderEffectResourceUsage_PS)
			{
				state = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			else
			{
				state = D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE;
				if ((signature->textures[t].usage | ShaderEffectResourceUsage_PS) != 0) state |= D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
			}
			Orbital_Video_D3D12_DepthStencil_ChangeState(depthStencil, handle->activeNodeIndex, state, handle->activeNode->commandList_Direct);
			++t;
		}

		// set read/write buffer states
		for (UINT i = 0; i != randomAccessBufferCount; ++i)
		{
			if (randomAccessTypes[i] == RandomAccessBufferType::RandomAccessBufferType_Texture)
			{
				Texture* texture = (Texture*)randomAccessBuffers[i];
				D3D12_RESOURCE_STATES state = {};
				state = D3D12_RESOURCE_STATE_UNORDERED_ACCESS;
				Orbital_Video_D3D12_Texture_ChangeState(texture, handle->activeNodeIndex, state, handle->activeNode->commandList_Direct);
			}
		}
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_SetRenderState(CommandList* handle, RenderState* renderState)
	{
		UINT activeNodeIndex = handle->activeNodeIndex;
		PipelineStateResources* resources = &renderState->resources;

		// update resources
		UpdateStateResources
		(
			handle, &renderState->shaderEffect->signature,
			renderState->resources.constantBufferCount, renderState->resources.constantBuffers,
			renderState->resources.textureCount, renderState->resources.textures,
			renderState->resources.textureDepthStencilCount, renderState->resources.textureDepthStencils,
			renderState->resources.randomAccessBufferCount, renderState->resources.randomAccessBuffers, renderState->resources.randomAccessTypes
		);

		// set vertex buffer states
		UINT vertexBufferCount = renderState->vertexBufferStreamer->vertexBufferCount;
		VertexBuffer** vertexBuffers = renderState->vertexBufferStreamer->vertexBuffers;
		for (UINT i = 0; i != vertexBufferCount; ++i) Orbital_Video_D3D12_VertexBuffer_ChangeState(vertexBuffers[i], handle->activeNodeIndex, D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER, handle->activeNode->commandList_Direct);

		// set index buffer states
		IndexBuffer* indexBuffer = renderState->indexBuffer;
		if (indexBuffer != NULL) Orbital_Video_D3D12_IndexBuffer_ChangeState(indexBuffer, handle->activeNodeIndex, D3D12_RESOURCE_STATE_INDEX_BUFFER, handle->activeNode->commandList_Direct);

		// bind shader resources
		handle->activeNode->commandList->SetGraphicsRootSignature(renderState->shaderEffect->signature.signature);

		UINT descIndex = 0;
		if (resources->nodes[activeNodeIndex].constantBufferHeap != NULL)
		{
			//handle->activeNode->commandList->SetDescriptorHeaps(1, &resources->nodes[activeNodeIndex].constantBufferHeap);
			//handle->activeNode->commandList->SetGraphicsRootDescriptorTable(descIndex, resources->nodes[activeNodeIndex].constantBufferGPUDescHandle);
			for (UINT i = 0; i != resources->constantBufferCount; ++i)
			{
				handle->activeNode->commandList->SetGraphicsRootConstantBufferView(descIndex, resources->constantBuffers[i]->nodes[activeNodeIndex].resource->GetGPUVirtualAddress());
				++descIndex;
			}
		}

		if (resources->nodes[activeNodeIndex].randomAccessBufferHeap != NULL)
		{
			handle->activeNode->commandList->SetDescriptorHeaps(1, &resources->nodes[activeNodeIndex].randomAccessBufferHeap);
			handle->activeNode->commandList->SetGraphicsRootDescriptorTable(descIndex, resources->nodes[activeNodeIndex].randomAccessBufferGPUDescHandle);
		}

		if (resources->nodes[activeNodeIndex].textureHeap != NULL)
		{
			handle->activeNode->commandList->SetDescriptorHeaps(1, &resources->nodes[activeNodeIndex].textureHeap);
			handle->activeNode->commandList->SetGraphicsRootDescriptorTable(descIndex, resources->nodes[activeNodeIndex].textureGPUDescHandle);
			++descIndex;
		}

		// enable render state
		handle->activeNode->commandList->SetPipelineState(renderState->state);
		
		// enable vertex / index buffers
		handle->activeNode->commandList->IASetPrimitiveTopology(renderState->topology);
		handle->activeNode->commandList->IASetVertexBuffers(0, vertexBufferCount, renderState->vertexBufferStreamer->nodes[activeNodeIndex].vertexBufferViews);
		if (indexBuffer != NULL) handle->activeNode->commandList->IASetIndexBuffer(&indexBuffer->nodes[activeNodeIndex].resourceView);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_SetComputeState(CommandList* handle, ComputeState* computeState)
	{
		UINT activeNodeIndex = handle->activeNodeIndex;
		PipelineStateResources* resources = &computeState->resources;

		// update resources
		UpdateStateResources
		(
			handle, &computeState->computeShader->signature,
			computeState->resources.constantBufferCount, computeState->resources.constantBuffers,
			computeState->resources.textureCount, computeState->resources.textures,
			computeState->resources.textureDepthStencilCount, computeState->resources.textureDepthStencils,
			computeState->resources.randomAccessBufferCount, computeState->resources.randomAccessBuffers, computeState->resources.randomAccessTypes
		);

		// bind shader resources
		handle->activeNode->commandList->SetComputeRootSignature(computeState->computeShader->signature.signature);

		UINT descIndex = 0;
		if (resources->nodes[activeNodeIndex].constantBufferHeap != NULL)
		{
			handle->activeNode->commandList->SetDescriptorHeaps(1, &resources->nodes[activeNodeIndex].constantBufferHeap);
			handle->activeNode->commandList->SetComputeRootDescriptorTable(descIndex, resources->nodes[activeNodeIndex].constantBufferGPUDescHandle);
			++descIndex;
		}

		if (resources->nodes[activeNodeIndex].textureHeap != NULL)
		{
			handle->activeNode->commandList->SetDescriptorHeaps(1, &resources->nodes[activeNodeIndex].textureHeap);
			handle->activeNode->commandList->SetComputeRootDescriptorTable(descIndex, resources->nodes[activeNodeIndex].textureGPUDescHandle);
			++descIndex;
		}

		if (resources->nodes[activeNodeIndex].randomAccessBufferHeap != NULL)
		{
			handle->activeNode->commandList->SetDescriptorHeaps(1, &resources->nodes[activeNodeIndex].randomAccessBufferHeap);
			handle->activeNode->commandList->SetComputeRootDescriptorTable(descIndex, resources->nodes[activeNodeIndex].randomAccessBufferGPUDescHandle);
		}

		// enable render state
		handle->activeNode->commandList->SetPipelineState(computeState->state);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawInstanced(CommandList* handle, UINT vertexOffset, UINT vertexCount, UINT instanceCount)
	{
		handle->activeNode->commandList->DrawInstanced(vertexCount, instanceCount, vertexOffset, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(CommandList* handle, UINT vertexOffset, UINT indexOffset, UINT indexCount, UINT instanceCount)
	{
		handle->activeNode->commandList->DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, 0);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_ExecuteComputeShader(CommandList* handle, UINT threadGroupCountX, UINT threadGroupCountY, UINT threadGroupCountZ)
	{
		handle->activeNode->commandList->Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_ResolveRenderTexture(CommandList* handle, Texture* srcRenderTexture, Texture* dstRenderTexture)
	{
		UINT activeNodeIndex = handle->activeNodeIndex;
		Orbital_Video_D3D12_Texture_ChangeState(srcRenderTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_SOURCE, handle->activeNode->commandList);
		Orbital_Video_D3D12_Texture_ChangeState(dstRenderTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RESOLVE_DEST, handle->activeNode->commandList);
		handle->activeNode->commandList->ResolveSubresource(dstRenderTexture->nodes[activeNodeIndex].resource, 0, srcRenderTexture->nodes[activeNodeIndex].resource, 0, srcRenderTexture->format);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_CopyTexture(CommandList* handle, Texture* srcTexture, Texture* dstTexture)
	{
		UINT activeNodeIndex = handle->activeNodeIndex;
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->activeNode->commandList);
		Orbital_Video_D3D12_Texture_ChangeState(dstTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->activeNode->commandList);
		handle->activeNode->commandList->CopyResource(dstTexture->nodes[activeNodeIndex].resource, srcTexture->nodes[activeNodeIndex].resource);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_CopyTextureRegion(CommandList* handle, Texture* srcTexture, Texture* dstTexture, UINT srcX, UINT srcY, UINT srcZ, UINT dstX, UINT dstY, UINT dstZ, UINT width, UINT height, UINT depth, UINT srcMipmapLevel, UINT dstMipmapLevel)
	{
		UINT activeNodeIndex = handle->activeNodeIndex;
		Orbital_Video_D3D12_Texture_ChangeState(srcTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_SOURCE, handle->activeNode->commandList);
		Orbital_Video_D3D12_Texture_ChangeState(dstTexture, activeNodeIndex, D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COPY_DEST, handle->activeNode->commandList);

		D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
		dstLoc.pResource = dstTexture->nodes[activeNodeIndex].resource;
		dstLoc.SubresourceIndex = dstMipmapLevel;

		D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
		srcLoc.pResource = srcTexture->nodes[activeNodeIndex].resource;
		srcLoc.SubresourceIndex = srcMipmapLevel;

		D3D12_BOX srcBox;
		srcBox.left = srcX;
		srcBox.right = srcX + width;
		srcBox.top = srcY;
		srcBox.bottom = srcY + height;
		srcBox.front = srcZ;
		srcBox.back = srcZ + depth;

		handle->activeNode->commandList->CopyTextureRegion(&dstLoc, dstX, dstY, dstZ, &srcLoc, &srcBox);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandList_Execute(CommandList* handle)
	{
		if (handle->activeNode->commandList_Direct != handle->activeNode->commandList)
		{
			ID3D12CommandList* commandLists[1] = { handle->activeNode->commandList_Direct };
			handle->activeNode->commandQueueRef_Direct->ExecuteCommandLists(1, commandLists);
			WaitForFence_CommandQueue(handle->activeNode->commandQueueRef_Direct, handle->activeNode->fence, handle->activeNode->fenceEvent, handle->activeNode->fenceValue);// make sure gpu has finished before we continue
		}

		ID3D12CommandList* commandLists[1] = { handle->activeNode->commandList };
		handle->activeNode->commandQueueRef->ExecuteCommandLists(1, commandLists);
		WaitForFence_CommandQueue(handle->activeNode->commandQueueRef, handle->activeNode->fence, handle->activeNode->fenceEvent, handle->activeNode->fenceValue);// make sure gpu has finished before we continue
	}
}