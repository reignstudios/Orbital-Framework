#include "Texture.h"

ORBITAL_EXPORT Texture* Orbital_Video_Vulkan_Texture_Create(Device* device)
{
	Texture* handle = (Texture*)calloc(1, sizeof(Texture));
	handle->device = device;
	return handle;
}

ORBITAL_EXPORT int Orbital_Video_Vulkan_Texture_Init()
{
	

	return 0;
}