#pragma once
#include <d3d12.h>
#include <dxgi1_6.h>
#include "../Orbital.Video/Interop/InteropStructures.h"

#ifdef _WIN32
#include <Windows.h>
#include <malloc.h>
#endif

#define ORBITAL_EXPORT __declspec(dllexport)