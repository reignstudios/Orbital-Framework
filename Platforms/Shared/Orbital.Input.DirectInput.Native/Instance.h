#pragma once
#include "Common.h"

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
#elif (DIRECTINPUT_VERSION >= 0x0700)
#define DI_INTERFACE IDirectInput7
#define DI_INTERFACE_ID IID_IDirectInput7
#elif (DIRECTINPUT_VERSION >= 0x0200)
#define DI_INTERFACE IDirectInput2
#define DI_INTERFACE_ID IID_IDirectInput2
#else
#define DI_INTERFACE IDirectInput
#define DI_INTERFACE_ID IID_IDirectInput
#endif

struct Instance
{
	DI_INTERFACE* diInterface;
};