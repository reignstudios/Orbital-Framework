#pragma once
#include "Device.h"

struct CommandList
{
	Device* device;
	ID3D12PipelineState* pipelineState;
	ID3D12GraphicsCommandList* commandList;
};