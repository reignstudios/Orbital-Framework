#pragma once
#include "Device.h"

typedef struct SwapChain
{
	Device* device;
	uint32_t currentRenderTargetIndex;
	VkSurfaceKHR surface;
	VkSwapchainKHR swapChain;
	uint32_t imageCount;
	VkImage* images;
	VkImageView* imageViews;
} SwapChain;