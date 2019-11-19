#pragma once
#include "Device.h"
#include "SwapChain.h"
#include "Texture.h"

struct RenderPass
{
	Device* device;
	SwapChain* swapChain;
	Texture* texture;
	UINT renderTargetCount;
	D3D12_RENDER_PASS_RENDER_TARGET_DESC* renderTargetDescs;
	D3D12_RENDER_PASS_DEPTH_STENCIL_DESC* depthStencilDesc;
};