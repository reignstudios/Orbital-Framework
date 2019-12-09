#pragma once
#include "Device.h"
#include "SwapChain.h"
#include "Texture.h"

struct RenderPass
{
	Device* device;
	SwapChain* swapChain;
	UINT renderTargetCount;
	DXGI_FORMAT* renderTargetFormats;
	DXGI_FORMAT depthStencilFormat;
	ID3D12Resource** renderTargetViews;
	D3D12_RENDER_PASS_RENDER_TARGET_DESC* renderTargetDescs;
	D3D12_RENDER_PASS_DEPTH_STENCIL_DESC* depthStencilDesc;
};