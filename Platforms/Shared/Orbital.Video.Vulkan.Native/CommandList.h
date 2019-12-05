#pragma once
#include "Device.h"

typedef struct CommandList
{
	Device* device;
	VkCommandBuffer commandBuffer;
	VkFence fence;
} CommandList;