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
		DI_DEVICE* diDevice = (DI_DEVICE*)pContext;

		if (pdidoi->dwType & DIDFT_AXIS)
		{
			DIPROPRANGE diprg;
			diprg.diph.dwSize = sizeof(DIPROPRANGE);
			diprg.diph.dwHeaderSize = sizeof(DIPROPHEADER);
			diprg.diph.dwHow = DIPH_BYID;
			diprg.diph.dwObj = pdidoi->dwType; // Specify the enumerated axis
			diprg.lMin = -1000;
			diprg.lMax = +1000;

			// Set the range for the axis
			if (FAILED(diDevice->SetProperty(DIPROP_RANGE, &diprg.diph))) return DIENUM_STOP;
		}

		if (pdidoi->guidType == GUID_XAxis)
		{
			return DIENUM_CONTINUE;
		}

		return DIENUM_CONTINUE;
	}

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Device_Init(Device* handle, Instance* instance, HWND window)
	{
		// query for joy interface
		DI_JOY_CONFIG* config = nullptr;
		if (FAILED(instance->diInterface->QueryInterface(DI_JOY_CONFIG_ID, (void**)&config))) return 0;

		// get primary device config if avaliable
		DIJOYCONFIG joyConfig = {};
		joyConfig.dwSize = sizeof(DIJOYCONFIG);
		HRESULT result = config->GetConfig(0, &joyConfig, DIJC_GUIDINSTANCE);
		if (FAILED(result)) memset(&joyConfig, 0, sizeof(DIJOYCONFIG));

		// enum all controllers
		EnumJoysticksContext enumContext;
		enumContext.instance = instance;
		enumContext.handle = handle;
		enumContext.joyConfig = &joyConfig;
		if (FAILED(instance->diInterface->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumJoysticksCallback, &enumContext, DIEDFL_ATTACHEDONLY))) return 0;

		// configure controllers
		if (window == nullptr) window = GetActiveWindow();
		if (window == nullptr) window = GetConsoleWindow();
		if (window == nullptr) return 0;
		for (int i = 0; i != handle->controllerCount; ++i)
		{
			// set data format
			if (FAILED(handle->controllers[i].diDevice->SetDataFormat(&c_dfDIJoystick2))) return 0;

			// set how the controller can be accessed
			if (FAILED(handle->controllers[i].diDevice->SetCooperativeLevel(window, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND)))
			{
				if (FAILED(handle->controllers[i].diDevice->SetCooperativeLevel(window, DISCL_NONEXCLUSIVE | DISCL_FOREGROUND)))
				{
					if (FAILED(handle->controllers[i].diDevice->SetCooperativeLevel(window, DISCL_EXCLUSIVE | DISCL_BACKGROUND)))
					{
						if (FAILED(handle->controllers[i].diDevice->SetCooperativeLevel(window, DISCL_EXCLUSIVE | DISCL_FOREGROUND))) return 0;
					}
				}
			}

			// enum controller capabilities
			if (FAILED(handle->controllers[i].diDevice->EnumObjects(EnumControllerStateCallback, handle->controllers[i].diDevice, DIDFT_ALL))) return 0;

			// acquire controller for use
			handle->controllers[i].diDevice->Acquire();// this can fail the first time so don't check for errors
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
	
	ORBITAL_EXPORT int Orbital_Video_DirectInput_Device_Update(Device* handle, int controllerIndex, DIJOYSTATE2* state, int* connected)
	{
		if (controllerIndex >= handle->controllerCount) return 0;

		Controller* controller = &handle->controllers[controllerIndex];
		DI_DEVICE* device = controller->diDevice;

		// try to re-connect to controller is not connected
		if (!controller->connected)
		{
			if (SUCCEEDED(device->Acquire()))
			{
				controller->connected = true;
			}
		}

		// poll controller state
		if (FAILED(device->Poll()))
		{
			// check for device lost
			HRESULT result = device->Acquire();
			if (result == DIERR_INPUTLOST)
			{
				controller->connected = false;
			}

			return 0;// no valid input this frame so continue
		}

		// get controller state
		if (FAILED(device->GetDeviceState(sizeof(DIJOYSTATE2), state))) return 0;

		// finish
		*connected = controller->connected;
		return 1;
	}
}