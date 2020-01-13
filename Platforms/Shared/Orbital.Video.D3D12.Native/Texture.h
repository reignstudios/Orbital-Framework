#pragma once
#include "Device.h"

struct Texture
{
	Device* device;
	TextureMode mode;
	ID3D12Resource* texture;
	//D3D12_CPU_DESCRIPTOR_HANDLE renderTargetDescHandle;
	DXGI_FORMAT format;
};