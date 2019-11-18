#pragma once
#include "Device.h"
#include "SwapChain.h"

typedef struct RenderPass
{
	Device* device;
	SwapChain* swapChain;
	VkRenderPass renderPass;
	uint32_t frameBufferCount;
	VkFramebuffer* frameBuffer;
	uint32_t width, height;
} RenderPass;