#pragma once
#include <vulkan/vulkan.h>
#include <vulkan/vk_sdk_platform.h>

#ifdef _WIN32
#include <Windows.h>
#include <vulkan/vulkan_win32.h>
#include <malloc.h>
#endif

#include "../Orbital.Video/Structures.h"

#define ORBITAL_EXPORT __declspec(dllexport)