#pragma once
#include "Common.h"

#if defined(_DEBUG)
#include <dxgidebug.h>
#endif

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
	IDXGIInfoQueue* infoQueue;
	IDXGIDebug* debugDXGI;
	IDXGIDebug1* debugDXGI1;
	ID3D12Debug* debugController;
	ID3D12Debug3* debugController3;
	#endif

	IDXGIFactory4* factory;
	D3D_FEATURE_LEVEL nativeMinFeatureLevel;
};