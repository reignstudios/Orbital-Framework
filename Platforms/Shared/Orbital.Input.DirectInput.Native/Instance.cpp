#include "Instance.h"

extern "C"
{
	ORBITAL_EXPORT Instance* Orbital_Video_DirectInput_Instance_Create()
	{
		return (Instance*)calloc(1, sizeof(Instance));
	}

	struct EnumJoysticksContext
	{
		Instance* handle;
		DIJOYCONFIG* joyConfig;
	};

	BOOL CALLBACK EnumControllersCallback(const DIDEVICEINSTANCE* pdidInstance, void* pContext)
	{
		EnumJoysticksContext* context = (EnumJoysticksContext*)pContext;
		Instance* handle = context->handle;

		// create DI controller device
		if (FAILED(handle->diInterface->CreateDevice(pdidInstance->guidInstance, &handle->devices[handle->deviceCount].diDevice, nullptr))) return DIENUM_CONTINUE;
		handle->devices[handle->deviceCount].connected = true;

		// check if controller is primary
		if (IsEqualGUID(context->joyConfig->guidInstance, pdidInstance->guidInstance)) handle->devices[handle->deviceCount].isPrimary = true;

		++handle->deviceCount;
		return handle->deviceCount >= 8 ? DIENUM_STOP : DIENUM_CONTINUE;
	}

	BOOL CALLBACK EnumControllerObjectsCallback(const DIDEVICEOBJECTINSTANCE* pdidoi, void* pContext)
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

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Instance_Init(Instance* handle, HWND window, FeatureLevel* minimumFeatureLevel)
	{
		// create interface
		#if DIRECTINPUT_VERSION > 0x0700
		handle->featureLevel = FeatureLevel::Level_8;
		if (FAILED(DirectInput8Create(GetModuleHandle(nullptr), DIRECTINPUT_VERSION, DI_INTERFACE_ID, (void**)&handle->diInterface, nullptr))) return 0;
		#else
		DirectInputCreateEx(GetModuleHandle(nullptr), DIRECTINPUT_VERSION, DI_INTERFACE_ID, (void**)&handle->diInterface, nullptr);
		#endif

		// report max feature level
		*minimumFeatureLevel = handle->featureLevel;

		// query for joy interface
		DI_JOY_CONFIG* config = nullptr;
		if (FAILED(handle->diInterface->QueryInterface(DI_JOY_CONFIG_ID, (void**)&config))) return 0;

		// get primary device config if avaliable
		DIJOYCONFIG joyConfig = {};
		joyConfig.dwSize = sizeof(DIJOYCONFIG);
		HRESULT result = config->GetConfig(0, &joyConfig, DIJC_GUIDINSTANCE);
		if (FAILED(result)) memset(&joyConfig, 0, sizeof(DIJOYCONFIG));

		// enum all controllers
		EnumJoysticksContext enumContext;
		enumContext.handle = handle;
		enumContext.joyConfig = &joyConfig;
		if (FAILED(handle->diInterface->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumControllersCallback, &enumContext, DIEDFL_ATTACHEDONLY))) return 0;

		// configure device
		if (window == nullptr) window = GetActiveWindow();// TODO: get this in C#
		if (window == nullptr) window = GetConsoleWindow();
		if (window == nullptr) return 0;
		// set data format
		for (int i = 0; i != handle->deviceCount; ++i)
		{
			if (FAILED(handle->devices[i].diDevice->SetDataFormat(&c_dfDIJoystick2))) return 0;

			// set how the device can be accessed
			if (FAILED(handle->devices[i].diDevice->SetCooperativeLevel(window, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND)))
			{
				if (FAILED(handle->devices[i].diDevice->SetCooperativeLevel(window, DISCL_NONEXCLUSIVE | DISCL_FOREGROUND)))
				{
					if (FAILED(handle->devices[i].diDevice->SetCooperativeLevel(window, DISCL_EXCLUSIVE | DISCL_BACKGROUND)))
					{
						if (FAILED(handle->devices[i].diDevice->SetCooperativeLevel(window, DISCL_EXCLUSIVE | DISCL_FOREGROUND))) return 0;
					}
				}
			}

			// enum device capabilities
			if (FAILED(handle->devices[i].diDevice->EnumObjects(EnumControllerObjectsCallback, handle->devices[i].diDevice, DIDFT_ALL))) return 0;

			// acquire device for use
			handle->devices[i].diDevice->Acquire();// this can fail the first time so don't check for errors
		}

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_DirectInput_Instance_Dispose(Instance* handle)
	{
		for (int i = 0; i != handle->deviceCount; ++i)
		{
			if (handle->devices[i].diDevice != nullptr)
			{
				handle->devices[i].diDevice->Unacquire();
				handle->devices[i].diDevice->Release();
				handle->devices[i].diDevice = nullptr;
			}
		}

		if (handle->diInterface != nullptr)
		{
			handle->diInterface->Release();
			handle->diInterface = nullptr;
		}
	}

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Instance_GetDeviceState(Instance* handle, int deviceIndex, DIJOYSTATE2* state, int* connected)
	{
		Device* device = &handle->devices[deviceIndex];
		DI_DEVICE* diDevice = device->diDevice;
		if (diDevice == nullptr) return 0;

		// try to re-connect to device is not connected
		if (!device->connected)
		{
			if (SUCCEEDED(diDevice->Acquire()))
			{
				device->connected = true;
			}
		}

		// poll device state
		if (FAILED(diDevice->Poll()))
		{
			// check for device lost
			HRESULT result = diDevice->Acquire();
			if (result == DIERR_INPUTLOST)
			{
				device->connected = false;
			}

			return 0;// no valid input this frame so continue
		}

		// get device state
		if (FAILED(diDevice->GetDeviceState(sizeof(DIJOYSTATE2), state))) return 0;

		// finish
		*connected = device->connected;
		return 1;
	}
}