#pragma once
#include "Instance.h"

typedef struct Device
{
	Instance* instance;
	VkDevice device;
	VkPhysicalDevice physicalDevice;
	VkPhysicalDeviceGroupProperties physicalDeviceGroup;
	uint32_t nativeFeatureLevel;
} Device;