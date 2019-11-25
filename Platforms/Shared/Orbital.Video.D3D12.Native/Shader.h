#pragma once
#include "Device.h"

struct Shader
{
	Device* device;
	D3D12_SHADER_BYTECODE bytecode;
};