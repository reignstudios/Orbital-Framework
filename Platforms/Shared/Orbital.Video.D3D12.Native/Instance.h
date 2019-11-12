#pragma once
#include "Common.h"

enum FeatureLevel
{
	Level_11_0,
	Level_11_1,
	Level_12_0,
	Level_12_1
};

struct Instance
{
	#if defined(_DEBUG)
	ID3D12Debug* debugController;
	#endif

	IDXGIFactory4* factory;
	D3D_FEATURE_LEVEL nativeMinFeatureLevel;
};