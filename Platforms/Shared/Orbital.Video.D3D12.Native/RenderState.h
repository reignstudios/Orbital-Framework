#pragma once
#include "Device.h"

struct RenderState
{
	Device* device;
	ID3D12PipelineState* state;
};