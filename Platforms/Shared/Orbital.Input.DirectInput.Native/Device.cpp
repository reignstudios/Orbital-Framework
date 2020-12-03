#include "Device.h"

extern "C"
{
	ORBITAL_EXPORT Device* Orbital_Video_DirectInput_Device_Create()
	{
		return (Device*)calloc(1, sizeof(Device));
	}

	struct EnumJoysticksContext
	{
		Instance* instance;
		Device* handle;
		DIJOYCONFIG* joyConfig;
	};

	BOOL CALLBACK EnumJoysticksCallback(const DIDEVICEINSTANCE* pdidInstance, void* pContext)
	{
		EnumJoysticksContext* context = (EnumJoysticksContext*)pContext;
		Instance* instance = context->instance;
		Device* handle = context->handle;
		
		// create DI controller device
		if (FAILED(instance->diInterface->CreateDevice(pdidInstance->guidInstance, &handle->controllers[handle->controllerCount].diDevice, nullptr))) return DIENUM_CONTINUE;
		handle->controllers[handle->controllerCount].connected = true;

		// check if controller is primary
		if (IsEqualGUID(context->joyConfig->guidInstance, pdidInstance->guidInstance)) handle->controllers[handle->controllerCount].isPrimary = true;

		++handle->controllerCount;
		return handle->controllerCount >= 8 ? DIENUM_STOP : DIENUM_CONTINUE;
	}

	BOOL CALLBACK EnumControllerStateCallback(const DIDEVICEOBJECTINSTANCE* pdidoi, void* pContext)
	{
		if (pdidoi->guidType == GUID_XAxis)
		{
			return DIENUM_CONTINUE;
		}

		return DIENUM_CONTINUE;
	}

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Device_Init(Device* handle, Instance* instance, FeatureLevel minimumFeatureLevel)
	{
		// query for joy interface
		DI_JOY_CONFIG* config = nullptr;
		if (FAILED(instance->diInterface->QueryInterface(DI_JOY_CONFIG_ID, (void**)&config))) return 0;

		// get primary device config
		DIJOYCONFIG joyConfig = { 0 };
		joyConfig.dwSize = sizeof(DIJOYCONFIG);
		if (FAILED(config->GetConfig(0, &joyConfig, DIJC_GUIDINSTANCE))) return 0;

		// enum all controllers
		EnumJoysticksContext enumContext;
		enumContext.instance = instance;
		enumContext.handle = handle;
		enumContext.joyConfig = &joyConfig;
		if (FAILED(instance->diInterface->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumJoysticksCallback, &enumContext, DIEDFL_ATTACHEDONLY))) return 0;

		// configure controllers
		HWND window = GetActiveWindow();
		for (int i = 0; i != handle->controllerCount; ++i)
		{
			// set how the controller can be accessed
			if (FAILED(handle->controllers[i].diDevice->SetCooperativeLevel(window, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND)))
			{
				if (FAILED(handle->controllers[i].diDevice->SetCooperativeLevel(window, DISCL_NONEXCLUSIVE | DISCL_FOREGROUND))) return 0;
			}

			// enum controller capabilities
			if (FAILED(handle->controllers[i].diDevice->EnumObjects(EnumControllerStateCallback, nullptr, DIDFT_ALL))) return 0;

			// acquire controller for use
			if (FAILED(handle->controllers[i].diDevice->Acquire())) return 0;
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_DirectInput_Device_Dispose(Device* handle)
	{
		for (int i = 0; i != 8; ++i)
		{
			if (handle->controllers[i].diDevice != nullptr)
			{
				handle->controllers[i].diDevice->Unacquire();
				handle->controllers[i].diDevice->Release();
				handle->controllers[i].diDevice = nullptr;
			}
		}
	}

	ORBITAL_EXPORT void Orbital_Video_DirectInput_Device_Update(Device* handle)
	{
		for (int i = 0; i != handle->controllerCount; ++i)
		{
			
		}
	}
}