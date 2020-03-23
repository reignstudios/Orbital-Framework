#pragma once
#include "Shader.h"
#include "ShaderSignature.h"

struct ComputeShader
{
	Device* device;
	D3D12_SHADER_BYTECODE bytecode;
	ShaderSignature signature;
};