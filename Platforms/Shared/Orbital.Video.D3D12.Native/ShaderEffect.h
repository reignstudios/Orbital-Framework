#pragma once
#include "Shader.h"
#include "ShaderSignature.h"

struct ShaderEffect
{
	Device* device;
	Shader *vs, *ps, *hs, *ds, *gs;
	ShaderSignature signature;
};