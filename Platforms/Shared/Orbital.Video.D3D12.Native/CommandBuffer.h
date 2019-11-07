#pragma once
#include "Common.h"

struct CommandBuffer
{
	ID3D12PipelineState* pipelineState;
	ID3D12GraphicsCommandList* commandList;
};