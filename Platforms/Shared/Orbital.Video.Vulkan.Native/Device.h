#pragma once
#include "Common.h"

typedef enum FeatureLevel
{
	FeatureLevel_Level_1_0,
	FeatureLevel_Level_1_1
} FeatureLevel;

typedef struct Device
{
	#if defined(_DEBUG)
	
	#endif

	VkInstance instance;
	VkPhysicalDevice device;
	VkPhysicalDeviceGroupProperties adapter;
} Device;