#pragma once
#include "Common.h"
#include "Device.h"

struct CommandBuffer
{
	Device* device;
	ID3D12PipelineState* pipelineState;
	ID3D12GraphicsCommandList* commandList;
};