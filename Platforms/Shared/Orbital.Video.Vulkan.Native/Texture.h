#pragma once
#include "Device.h"

typedef struct Texture
{
	Device* device;
	VkImage image;
	VkImageView imageView;
	uint32_t width, height, depth;
	VkFormat format;
} Texture;