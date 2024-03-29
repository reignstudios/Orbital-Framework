#include "Instance.h"
#include <dbt.h>

#if (DIRECTINPUT_VERSION >= 0x0800)
#include "../../Shared/Orbital.Input.XInput/FindXInputControllers.h"
#endif

extern "C"
{
	ORBITAL_EXPORT Instance* Orbital_Video_DirectInput_Instance_Create(int ignoreXInputDevices)
	{
		Instance* handle = (Instance*)calloc(1, sizeof(Instance));
		handle->ignoreXInputDevices = ignoreXInputDevices != 0;
		return handle;
	}

	struct EnumControllersContext
	{
		Instance* handle;
		DIJOYCONFIG* joyConfig;
	};

	BOOL CALLBACK EnumControllersCallback(const DIDEVICEINSTANCE* pdidInstance, void* pContext)
	{
		EnumControllersContext* context = (EnumControllersContext*)pContext;
		Instance* handle = context->handle;
		Device* device = &handle->devices[handle->deviceCount];

		#if (DIRECTINPUT_VERSION >= 0x0800)
		if (handle->ignoreXInputDevices)
		{
			if (XInputVIDPIDExists(pdidInstance->guidProduct.Data1)) return DIENUM_CONTINUE;
		}
		#endif

		// only create device if its not connected
		if (!device->connected)
		{
			// dispose old instance
			if (device->diDevice != nullptr)
			{
				device->diDevice->Unacquire();
				device->diDevice->Release();
				memset(device, 0, sizeof(Device));
			}

			// create DI controller device
			if (FAILED(handle->diInterface->CreateDevice(pdidInstance->guidInstance, &device->diDevice, nullptr))) return DIENUM_CONTINUE;
			device->connected = true;
			device->newConnection = true;

			// check if controller is primary
			if (IsEqualGUID(context->joyConfig->guidInstance, pdidInstance->guidInstance)) device->isPrimary = true;

			// check if Force-Feedback is supported
			if (!IsEqualGUID(pdidInstance->guidFFDriver, GUID_NULL)) device->supportsForceFeedback = true;

			// copy product id
			device->productID = pdidInstance->guidProduct;

			// copy produce name
			wcscpy_s(device->productName, pdidInstance->tszProductName);

			// get device type
			device->type = pdidInstance->dwDevType;
		}

		// finish
		++handle->deviceCount;
		return handle->deviceCount >= 8 ? DIENUM_STOP : DIENUM_CONTINUE;
	}

	struct EnumControllerObjectsContext
	{
		Instance* handle;
		Device* device;
	};

	BOOL CALLBACK EnumControllerObjectsCallback(const DIDEVICEOBJECTINSTANCE* pdidoi, void* pContext)
	{
		EnumControllerObjectsContext* context = (EnumControllerObjectsContext*)pContext;
		Device* device = context->device;

		// set the range for any axis to 1000
		if ((pdidoi->dwType & DIDFT_AXIS) != 0)
		{
			DIPROPRANGE diprg;
			diprg.diph.dwSize = sizeof(DIPROPRANGE);
			diprg.diph.dwHeaderSize = sizeof(DIPROPHEADER);
			diprg.diph.dwHow = DIPH_BYID;
			diprg.diph.dwObj = pdidoi->dwType;// specify the enumerated axis type
			diprg.lMin = -1000;
			diprg.lMax = +1000;
			if (FAILED(device->diDevice->SetProperty(DIPROP_RANGE, &diprg.diph))) return DIENUM_STOP;
		}

		// gather object counts
		if (pdidoi->guidType == GUID_Button) ++device->buttonCount;
		else if (pdidoi->guidType == GUID_Key) ++device->keyCount;
		else if (pdidoi->guidType == GUID_POV) ++device->povCount;
		else if (pdidoi->guidType == GUID_Slider) ++device->sliderCount;
		else if (pdidoi->guidType == GUID_XAxis) ++device->xAxisCount;
		else if (pdidoi->guidType == GUID_YAxis) ++device->yAxisCount;
		else if (pdidoi->guidType == GUID_ZAxis) ++device->zAxisCount;
		else if (pdidoi->guidType == GUID_RxAxis) ++device->rxAxisCount;
		else if (pdidoi->guidType == GUID_RyAxis) ++device->ryAxisCount;
		else if (pdidoi->guidType == GUID_RzAxis) ++device->rzAxisCount;

		return DIENUM_CONTINUE;
	}

	bool refreshInputDevices = false;
	LRESULT WndProc_Hook(int code, WPARAM wParam, LPARAM lParam)
	{
		// invalid code skip
		if (code < 0) return CallNextHookEx(NULL, code, wParam, lParam);

		// check if device was added/removed
		PCWPSTRUCT pMsg = PCWPSTRUCT(lParam);
		if (pMsg->message == WM_DEVICECHANGE)
		{
			switch (pMsg->wParam)
			{
			case DBT_DEVNODES_CHANGED:
				refreshInputDevices = true;
				break;

			case DBT_DEVICEARRIVAL:
				refreshInputDevices = true;
				break;

			case DBT_DEVICEREMOVECOMPLETE:
				refreshInputDevices = true;
				break;
			}
		}

		// continue as normal
		return CallNextHookEx(NULL, code, wParam, lParam);
	}

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Instance_RefreshDevices(Instance* handle)
	{
		// scan for XInput controllers if needed
		#if (DIRECTINPUT_VERSION >= 0x0800)
		if (handle->ignoreXInputDevices != 0)
		{
			if (!FindXInputControllers()) return 0;
		}
		#endif

		// dispose existing devices
		for (int i = 0; i != 8; ++i)
		{
			Device* device = &handle->devices[i];
			if (device->diDevice != nullptr)
			{
				device->diDevice->Unacquire();
				device->diDevice->Release();
				memset(device, 0, sizeof(Device));
			}
		}

		// get primary device config if avaliable
		DIJOYCONFIG joyConfig = {};
		joyConfig.dwSize = sizeof(DIJOYCONFIG);
		HRESULT result = handle->config->GetConfig(0, &joyConfig, DIJC_GUIDINSTANCE);
		if (FAILED(result)) memset(&joyConfig, 0, sizeof(DIJOYCONFIG));
		
		// enum all controllers
		handle->deviceCount = 0;
		EnumControllersContext enumContext;
		enumContext.handle = handle;
		enumContext.joyConfig = &joyConfig;
		#if (DIRECTINPUT_VERSION >= 0x0800)
		if (FAILED(handle->diInterface->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumControllersCallback, &enumContext, DIEDFL_ATTACHEDONLY))) return 0;
		#else
		if (FAILED(handle->diInterface->EnumDevices(DIDEVTYPE_JOYSTICK, EnumControllersCallback, &enumContext, DIEDFL_ATTACHEDONLY))) return 0;
		#endif

		// configure device
		for (int i = 0; i != handle->deviceCount; ++i)
		{
			Device* device = &handle->devices[i];

			// only configure if new connection
			if (!device->newConnection) continue;
			device->newConnection = false;

			// set data format
			if (FAILED(device->diDevice->SetDataFormat(&c_dfDIJoystick2))) return 0;

			// set how the device can be accessed
			if (FAILED(device->diDevice->SetCooperativeLevel(handle->window, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND)))
			{
				if (FAILED(device->diDevice->SetCooperativeLevel(handle->window, DISCL_NONEXCLUSIVE | DISCL_FOREGROUND)))
				{
					if (FAILED(device->diDevice->SetCooperativeLevel(handle->window, DISCL_EXCLUSIVE | DISCL_BACKGROUND)))
					{
						if (FAILED(device->diDevice->SetCooperativeLevel(handle->window, DISCL_EXCLUSIVE | DISCL_FOREGROUND))) return 0;
					}
				}
			}

			// enum device capabilities
			EnumControllerObjectsContext enumObjectsContext;
			enumObjectsContext.handle = handle;
			enumObjectsContext.device = device;
			if (FAILED(device->diDevice->EnumObjects(EnumControllerObjectsCallback, &enumObjectsContext, DIDFT_ALL))) return 0;

			// acquire device for use
			device->diDevice->Acquire();// this can fail the first time so don't check for errors
		}

		return 1;
	}

	ORBITAL_EXPORT int Orbital_Video_DirectInput_Instance_Init(Instance* handle, HWND window, FeatureLevel* minimumFeatureLevel)
	{
		if (window == nullptr) window = GetActiveWindow();
		if (window == nullptr) window = GetConsoleWindow();
		if (window == nullptr) return 0;
		handle->window = window;

		// create interface
		#if (DIRECTINPUT_VERSION >= 0x0800)
		handle->featureLevel = FeatureLevel::Level_8;
		if (FAILED(DirectInput8Create(GetModuleHandle(nullptr), DIRECTINPUT_VERSION, DI_INTERFACE_ID, (void**)&handle->diInterface, nullptr))) return 0;
		#else
		DirectInputCreateEx(GetModuleHandle(nullptr), DIRECTINPUT_VERSION, DI_INTERFACE_ID, (void**)&handle->diInterface, nullptr);
		#endif
		
		// report max feature level
		*minimumFeatureLevel = handle->featureLevel;

		// query for joy interface
		if (FAILED(handle->diInterface->QueryInterface(DI_JOY_CONFIG_ID, (void**)&handle->config))) return 0;

		// gather existing devices
		Orbital_Video_DirectInput_Instance_RefreshDevices(handle);

		// hook WinProc to watch for device changes
		HMODULE module = GetModuleHandleW(NULL);
		DWORD threadID = GetCurrentThreadId();
		handle->winprocHook = SetWindowsHookExW(WH_CALLWNDPROC, (HOOKPROC)&WndProc_Hook, module, threadID);

		return 1;
	}

	ORBITAL_EXPORT void Orbital_Video_DirectInput_Instance_Dispose(Instance* handle)
	{
		if (handle->winprocHook != nullptr)
		{
			UnhookWindowsHookEx(handle->winprocHook);
			handle->winprocHook = nullptr;
		}

		if (handle->config != nullptr)
		{
			handle->config->Release();
			handle->config = nullptr;
		}

		for (int i = 0; i != handle->deviceCount; ++i)
		{
			Device* device = &handle->devices[i];
			if (device->diDevice != nullptr)
			{
				device->diDevice->Unacquire();
				device->diDevice->Release();
				memset(device, 0, sizeof(Device));
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
		// refresh devices
		if (refreshInputDevices)
		{
			refreshInputDevices = false;
			Orbital_Video_DirectInput_Instance_RefreshDevices(handle);
		}

		// get current device
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

	struct DeviceInfo
	{
		GUID productID;
		WCHAR* productName;
		DWORD type;
		int supportsForceFeedback;
		int isPrimary;

		int buttonCount;
		int keyCount;
		int povCount;
		int sliderCount;
		int xAxisCount, yAxisCount, zAxisCount;
		int rxAxisCount, ryAxisCount, rzAxisCount;
	};

	ORBITAL_EXPORT void Orbital_Video_DirectInput_Instance_GetDeviceInfo(Instance* handle, int deviceIndex, DeviceInfo* info)
	{
		Device* device = &handle->devices[deviceIndex];

		info->productID = device->productID;
		info->productName = device->productName;
		info->type = device->type;
		info->supportsForceFeedback = device->supportsForceFeedback;
		info->isPrimary = device->isPrimary;

		info->buttonCount = device->buttonCount;
		info->keyCount = device->keyCount;
		info->povCount = device->povCount;
		info->sliderCount = device->sliderCount;
		info->xAxisCount = device->xAxisCount;
		info->yAxisCount = device->yAxisCount;
		info->zAxisCount = device->zAxisCount;
		info->rxAxisCount = device->rxAxisCount;
		info->ryAxisCount = device->ryAxisCount;
		info->rzAxisCount = device->rzAxisCount;
	}
}