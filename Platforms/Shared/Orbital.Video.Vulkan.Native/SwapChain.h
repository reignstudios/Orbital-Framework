#pragma once
#include "Device.h"

typedef struct SwapChain
{
	Device* device;
	uint32_t bufferCount, currentRenderTargetIndex;
	uint32_t width, height;
	VkSurfaceKHR surface;
	VkSwapchainKHR swapChain;
	VkImage* images;
	VkImageView* imageViews;
	VkFramebuffer* frameBuffers;
	VkRenderPass renderPass;
	VkFence fence;
} SwapChain;