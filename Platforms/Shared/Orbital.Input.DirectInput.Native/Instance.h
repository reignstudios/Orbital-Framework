#pragma once
#include "Common.h"
#include <Windows.h>

enum FeatureLevel
{
	Level_1,
	Level_2,
	Level_7,
	Level_8
};

#if (DIRECTINPUT_VERSION >= 0x0800)
#define DI_INTERFACE IDirectInput8
#define DI_INTERFACE_ID IID_IDirectInput8
#define DI_DEVICE IDirectInputDevice8
#elif (DIRECTINPUT_VERSION >= 0x0700)
#define DI_INTERFACE IDirectInput7
#define DI_INTERFACE_ID IID_IDirectInput7
#define DI_DEVICE IDirectInputDevice7
#elif (DIRECTINPUT_VERSION >= 0x0200)
#define DI_INTERFACE IDirectInput2
#define DI_INTERFACE_ID IID_IDirectInput2
#define DI_DEVICE IDirectInputDevice2
#else
#define DI_INTERFACE IDirectInput
#define DI_INTERFACE_ID IID_IDirectInput
#define DI_DEVICE IDirectInputDevice
#endif

#if (DIRECTINPUT_VERSION >= 0x0800)
#define DI_JOY_CONFIG IDirectInputJoyConfig8
#define DI_JOY_CONFIG_ID IID_IDirectInputJoyConfig8
#else
#define DI_JOY_CONFIG IDirectInputJoyConfig
#define DI_JOY_CONFIG_ID IID_IDirectInputJoyConfig
#endif

struct Device
{
	DI_DEVICE* diDevice;
	bool newConnection, connected, isPrimary;
	GUID productID;
	WCHAR productName[MAX_PATH];
	bool supportsForceFeedback;
	DWORD type;

	int buttonCount;
	int keyCount;
	int povCount;
	int sliderCount;
	int xAxisCount, yAxisCount, zAxisCount;
	int rxAxisCount, ryAxisCount, rzAxisCount;
};

struct Instance
{
	DI_INTERFACE* diInterface;
	DI_JOY_CONFIG* config;
	HWND window;
	HHOOK winprocHook;
	FeatureLevel featureLevel;
	bool ignoreXInputDevices;

	Device devices[8];
	int deviceCount;
};