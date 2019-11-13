#pragma once
#include "Instance.h"

typedef struct Device
{
	uint32_t nativeFeatureLevel;
	Instance* instance;
	VkDevice device;
	VkPhysicalDevice physicalDevice;
	VkPhysicalDeviceGroupProperties physicalDeviceGroup;
	VkCommandPool commandPool;
} Device;