#include "Instance.h"

extern "C"
{
	ORBITAL_EXPORT Instance* Orbital_Video_DirectInput_Instance_Create()
	{
		return (Instance*)calloc(1, sizeof(Instance));
	}

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Instance_Init(Instance* handle, FeatureLevel* minimumFeatureLevel)
	{
		// create interface
		#if DIRECTINPUT_VERSION > 0x0700
		handle->featureLevel = FeatureLevel::Level_8;
		if (FAILED(DirectInput8Create(GetModuleHandle(nullptr), DIRECTINPUT_VERSION, DI_INTERFACE_ID, (void**)&handle->diInterface, nullptr))) return 0;
		#else
		DirectInputCreateEx(GetModuleHandle(nullptr), DIRECTINPUT_VERSION, DI_INTERFACE_ID, (void**)&handle->diInterface, nullptr);
		#endif

		*minimumFeatureLevel = handle->featureLevel;
		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_DirectInput_Instance_Dispose(Instance* handle)
	{
		if (handle->diInterface != nullptr)
		{
			handle->diInterface->Release();
			handle->diInterface = nullptr;
		}
	}
}