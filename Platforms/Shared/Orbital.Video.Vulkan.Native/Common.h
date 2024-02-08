#pragma once
#include <vulkan/vulkan.h>

#ifdef _WIN32
#include <Windows.h>
#include <vulkan/vulkan_win32.h>
#include <malloc.h>
#endif

#include "../Orbital.Video/Interop/InteropStructures.h"

#define ORBITAL_EXPORT __declspec(dllexport)