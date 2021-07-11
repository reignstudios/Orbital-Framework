#pragma once
#include <SetupAPI.h>
#include <string.h>
#include <string>

DWORD xinputVidPid[16];
int xinputVidPidCount;

bool FindXInputControllers()
{
	// get devices handle
	GUID classGUID = {};
	HDEVINFO classHandle = SetupDiGetClassDevsW(&classGUID, nullptr, nullptr, DIGCF_PRESENT | DIGCF_ALLCLASSES);
	if (classHandle == INVALID_HANDLE_VALUE || classHandle == nullptr) return false;

	// enumerate over all devices
	DWORD propType = 0;
	byte propBuffer[256];
	DWORD propBufferSize = 0;

	SP_DEVINFO_DATA devInfo = {};
	devInfo.cbSize = sizeof(SP_DEVINFO_DATA);

	xinputVidPidCount = 0;
	DWORD deviceIndex = 0;
	while (SetupDiEnumDeviceInfo(classHandle, deviceIndex, &devInfo))
	{
		if (SetupDiGetDeviceRegistryPropertyW(classHandle, &devInfo, SPDRP_HARDWAREID, &propType, propBuffer, sizeof(propBuffer), &propBufferSize))
		{
			wchar_t* name = (wchar_t*)propBuffer;
			DWORD offset = 0;
			while (offset < propBufferSize)
			{
				// check if this is a XInput device
				if (wcsstr(name, L"IG_") && xinputVidPidCount < (sizeof(xinputVidPid) / sizeof(DWORD)))
				{
					DWORD dwPid = 0, dwVid = 0;
					WCHAR* strVid = wcsstr(name, L"VID_");
					if (strVid && swscanf_s(strVid, L"VID_%4X", &dwVid) != 1) dwVid = 0;

					WCHAR* strPid = wcsstr(name, L"PID_");
					if (strPid && swscanf_s(strPid, L"PID_%4X", &dwPid) != 1) dwPid = 0;

					xinputVidPid[xinputVidPidCount] = MAKELONG(dwVid, dwPid);
					++xinputVidPidCount;
					break;
				}

				// offset to next string
				size_t length = wcslen(name) + 1;
				name += length;
				offset += length * sizeof(wchar_t);
			}
		}
		++deviceIndex;
	}

	// destroy handle
	SetupDiDestroyDeviceInfoList(classHandle);
	return true;
}

bool XInputVIDPIDExists(DWORD vidPid)
{
	for (int i = 0; i != xinputVidPidCount; ++i)
	{
		if (vidPid == xinputVidPid[i])
		{
			return true;
		}
	}
	return false;
}