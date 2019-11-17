#pragma once
#include "Device.h"

typedef struct Texture
{
	Device* device;
	VkImage image;
	VkImageView imageView;
} Texture;