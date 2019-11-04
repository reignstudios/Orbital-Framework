#include "pch.h"
#include "Device.h"
#include <dxgi1_6.h>

bool Device::Init()
{
	UINT dxgiFactoryFlags = 0;

#if defined(_DEBUG)
	if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
	{
		debugController->EnableDebugLayer();
		dxgiFactoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
	}
#endif



	return true;
}

void Device::Dispose()
{
	if (device != NULL)
	{
		device->Release();
		device = NULL;
	}

	if (debugController != NULL)
	{
		debugController->Release();
		debugController = NULL;
	}
}

extern "C"
{
	bool Orbital_Video_D3D12_Device_Init(Device* device)
	{
		return device->Init();
	}

	void Orbital_Video_D3D12_Device_Dispose(Device* device)
	{
		device->Dispose();
	}
}