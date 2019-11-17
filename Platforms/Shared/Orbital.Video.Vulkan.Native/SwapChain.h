#pragma once
#include "Device.h"

typedef struct SwapChain
{
	Device* device;
	VkSurfaceKHR surface;
	VkSwapchainKHR swapChain;
	uint32_t imageViewCount;
	VkImageView* imageViews;
} SwapChain;