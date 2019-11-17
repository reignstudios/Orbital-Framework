#pragma once
#include "Device.h"

typedef struct RenderPass
{
	Device* device;
	VkRenderPass renderPass;
	VkFramebuffer frameBuffer;
	uint32_t width, height;
} RenderPass;