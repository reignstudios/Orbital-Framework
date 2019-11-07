#include "CommandBuffer.h"
#include "Device.h"
#include "SwapChain.h"

extern "C"
{
	ORBITAL_EXPORT CommandBuffer* Orbital_Video_D3D12_CommandBuffer_Create()
	{
		return (CommandBuffer*)calloc(1, sizeof(CommandBuffer));
	}

	ORBITAL_EXPORT bool Orbital_Video_D3D12_CommandBuffer_Init(CommandBuffer* handle, Device* device)
	{
		if (FAILED(device->device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, device->commandAllocator, nullptr, IID_PPV_ARGS(&handle->commandList)))) return false;
		if (FAILED(handle->commandList->Close())) return false;// make sure this is closed as it defaults to open for writing

		return true;
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_Dispose(CommandBuffer* handle)
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

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_Start(CommandBuffer* handle, Device* device)
	{
		handle->commandList->Reset(device->commandAllocator, handle->pipelineState);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_Finish(CommandBuffer* handle)
	{
		handle->commandList->Close();
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_EnableSwapChainRenderTarget(CommandBuffer* handle, SwapChain* swapChain)
	{
		D3D12_RESOURCE_BARRIER barrier = {};
		barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
		barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
		barrier.Transition.pResource = swapChain->renderTargetViews[swapChain->currentRenderTargetIndex];
		barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
		barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
		barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
		handle->commandList->ResourceBarrier(1, &barrier);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_EnableSwapChainPresent(CommandBuffer* handle, SwapChain* swapChain)
	{
		D3D12_RESOURCE_BARRIER barrier = {};
		barrier.Type = D3D12_RESOURCE_BARRIER_TYPE::D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
		barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS::D3D12_RESOURCE_BARRIER_FLAG_NONE;
		barrier.Transition.pResource = swapChain->renderTargetViews[swapChain->currentRenderTargetIndex];
		barrier.Transition.StateBefore = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_RENDER_TARGET;
		barrier.Transition.StateAfter = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_PRESENT;
		barrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;
		handle->commandList->ResourceBarrier(1, &barrier);
	}

	ORBITAL_EXPORT void Orbital_Video_D3D12_CommandBuffer_ClearSwapChainRenderTarget(CommandBuffer* handle, SwapChain* swapChain, float r, float g, float b, float a)
	{
		float rgba[4] = {r, g, b, a};
		handle->commandList->ClearRenderTargetView(swapChain->renderTargetDescHandles[swapChain->currentRenderTargetIndex], rgba, 0, NULL);
	}
}