#pragma once
#include "Common.h"

typedef enum FeatureLevel
{
	FeatureLevel_Level_1_0,
	FeatureLevel_Level_1_1,
	FeatureLevel_Level_1_2
} FeatureLevel;

typedef struct Instance
{
	VkInstance instance;
	uint32_t nativeMinFeatureLevel, nativeMaxFeatureLevel;
} Instance;