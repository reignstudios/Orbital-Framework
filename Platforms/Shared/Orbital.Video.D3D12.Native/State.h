#pragma once
#include "Device.h"

struct State
{
	Device* device;
	ID3D12PipelineState* state;
};