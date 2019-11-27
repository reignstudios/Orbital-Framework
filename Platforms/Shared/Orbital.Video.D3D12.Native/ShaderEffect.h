#pragma once
#include "Shader.h"

struct ShaderEffect
{
	Device* device;
	Shader *vs, *ps, *hs, *ds, *gs;
	ID3D12RootSignature** signatures;// signature per GPU node
};