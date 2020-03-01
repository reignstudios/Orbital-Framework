#pragma once
#include "Device.h"
#include "SwapChain.h"
#include "Texture.h"
#include "DepthStencil.h"

struct RenderPass
{
	Device* device;
	SwapChain* swapChain;
	Texture** renderTextures;
	DepthStencil* depthStencil;
	UINT renderTargetCount;
	DXGI_FORMAT* renderTargetFormats;
	DXGI_FORMAT depthStencilFormat;
	ID3D12Resource** renderTargetResources;
	D3D12_RENDER_PASS_RENDER_TARGET_DESC* renderTargetDescs;
	D3D12_RENDER_PASS_DEPTH_STENCIL_DESC* depthStencilDesc;
};