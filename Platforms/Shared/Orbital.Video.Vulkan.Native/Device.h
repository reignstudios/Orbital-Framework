#pragma once
#include "Instance.h"

typedef struct Device
{
	DeviceType type;
	uint32_t nativeFeatureLevel;
	VkPhysicalDevice physicalDevice;
	VkPhysicalDeviceGroupProperties physicalDeviceGroup;
	VkPhysicalDeviceFeatures physicalDeviceFeatures;
	uint32_t queueFamilyIndex;

	Instance* instance;
	VkDevice device;
	VkQueue queue;
	VkCommandPool commandPool;

	uint32_t activeFenceCount;
	VkFence activeFences[1024];
} Device;

void Device_AddFence(Device* device, VkFence fence);