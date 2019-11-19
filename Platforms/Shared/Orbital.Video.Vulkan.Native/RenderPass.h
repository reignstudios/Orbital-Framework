#pragma once
#include "Device.h"
#include "SwapChain.h"
#include "Texture.h"

typedef struct RenderPass
{
	Device* device;
	SwapChain* swapChain;
	Texture* texture;
	VkRenderPass renderPass;
	uint32_t frameBufferCount;
	VkFramebuffer* frameBuffers;
	uint32_t width, height;

	char clearColor;
	float clearColorValue[4];
} RenderPass;