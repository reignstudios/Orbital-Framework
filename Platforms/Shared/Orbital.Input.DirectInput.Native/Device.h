#pragma once
#include "Instance.h"

#if (DIRECTINPUT_VERSION >= 0x0800)
#define DI_JOY_CONFIG IDirectInputJoyConfig8
#define DI_JOY_CONFIG_ID IID_IDirectInputJoyConfig8
#else
#define DI_JOY_CONFIG IDirectInputJoyConfig
#define DI_JOY_CONFIG_ID IID_IDirectInputJoyConfig
#endif

struct Controller
{
	bool connected, isPrimary;
	DI_DEVICE* diDevice;
};

struct Device
{
	Controller controllers[8];
	int controllerCount;
};