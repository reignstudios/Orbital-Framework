using System;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;
using ULONG_PTR = System.UIntPtr;
using GUID = System.Guid;
using HDEVINFO = System.IntPtr;
using HWND = System.IntPtr;
using BOOL = System.Int32;
using WCHAR = System.Int16;
using BYTE = System.Byte;
using DI_FUNCTION = System.UInt32;
using DEVPROPTYPE = System.UInt32;
using DEVPROPGUID = System.Guid;
using DEVPROPID = System.UInt32;
using HKEY = System.IntPtr;
using REGSAM = System.UInt32;

// 'GUID_DEVCLASS_????' reference
// #include <devguid.h>
// https://docs.microsoft.com/en-us/windows-hardware/drivers/install/system-defined-device-setup-classes-available-to-vendors

namespace Orbital.OS.Native
{
	public static class SetupAPI
	{
		private const string lib = "SetupAPI.dll";

		[DllImport(lib)]
		public static unsafe extern HDEVINFO SetupDiGetClassDevsW(ref GUID ClassGuid, WCHAR* Enumerator, HWND hwndParent, DWORD Flags);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiDestroyDeviceInfoList(HDEVINFO DeviceInfoSet);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiEnumDeviceInterfaces(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, ref GUID InterfaceClassGuid, DWORD MemberIndex, SP_DEVICE_INTERFACE_DATA* DeviceInterfaceData);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiGetDeviceInterfaceDetailW(HDEVINFO DeviceInfoSet, SP_DEVICE_INTERFACE_DATA* DeviceInterfaceData, SP_DEVICE_INTERFACE_DETAIL_DATA_W* DeviceInterfaceDetailData, DWORD DeviceInterfaceDetailDataSize, DWORD* RequiredSize, SP_DEVINFO_DATA* DeviceInfoData);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiCallClassInstaller(DI_FUNCTION InstallFunction, HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiEnumDeviceInfo(HDEVINFO DeviceInfoSet, DWORD MemberIndex, SP_DEVINFO_DATA* DeviceInfoData);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiSetClassInstallParamsW(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, SP_CLASSINSTALL_HEADER* ClassInstallParams, DWORD ClassInstallParamsSize);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiGetDeviceInstanceIdW(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, char* DeviceInstanceId, DWORD DeviceInstanceIdSize, DWORD* RequiredSize);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiGetDevicePropertyW(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, DEVPROPKEY* PropertyKey, DEVPROPTYPE* PropertyType, BYTE* PropertyBuffer, DWORD PropertyBufferSize, DWORD* RequiredSize, DWORD Flags);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiGetDeviceRegistryPropertyW(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, DWORD Property, DWORD* PropertyRegDataType, BYTE* PropertyBuffer, DWORD PropertyBufferSize, DWORD* RequiredSize);

		[DllImport(lib)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static unsafe extern bool SetupDiRemoveDevice(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData);

		[DllImport(lib)]
		public static unsafe extern HKEY SetupDiOpenDevRegKey(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, DWORD Scope, DWORD HwProfile, DWORD KeyType, REGSAM samDesired);

		public static DEVPROPKEY DEVPKEY_Device_FriendlyName
		{
			get
			{
				return new DEVPROPKEY()
				{
					fmtid = new GUID(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0),
					pid = 14
				};
			}
		}

		public static DEVPROPKEY DEVPKEY_Device_BusReportedDeviceDesc
		{
			get
			{
				return new DEVPROPKEY()
				{
					fmtid = new GUID(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2),
					pid = 4
				};
			}
		}

		public const uint DIGCF_DEFAULT = 0x00000001;
		public const uint DIGCF_PRESENT = 0x00000002;
		public const uint DIGCF_ALLCLASSES = 0x00000004;
		public const uint DIGCF_PROFILE = 0x00000008;
		public const uint DIGCF_DEVICEINTERFACE = 0x00000010;

		public const uint DICS_ENABLE = 0x00000001;
		public const uint DICS_DISABLE = 0x00000002;
		public const uint DICS_PROPCHANGE = 0x00000003;
		public const uint DICS_START = 0x00000004;
		public const uint DICS_STOP = 0x00000005;

		public const uint DICS_FLAG_GLOBAL = 0x00000001;
		public const uint DICS_FLAG_CONFIGSPECIFIC = 0x00000002;
		public const uint DICS_FLAG_CONFIGGENERAL = 0x00000004;

		public const uint DIF_PROPERTYCHANGE = 0x00000012;

		public const uint SPDRP_INSTALL_STATE = 0x00000022;
		public const uint SPDRP_FRIENDLYNAME = 0x0000000C;
		public const uint SPDRP_HARDWAREID = 0x00000001;

		public const uint DIREG_DEV = 0x00000001;
		public const uint DIREG_DRV = 0x00000002;
		public const uint DIREG_BOTH = 0x00000004;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEVPROPKEY
	{
		public DEVPROPGUID fmtid;
		public DEVPROPID pid;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SP_CLASSINSTALL_HEADER
	{
		public DWORD cbSize;
		public DI_FUNCTION InstallFunction;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SP_REMOVEDEVICE_PARAMS
	{
		public SP_CLASSINSTALL_HEADER ClassInstallHeader;
		public DWORD Scope;
		public DWORD HwProfile;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SP_PROPCHANGE_PARAMS
	{
		public SP_CLASSINSTALL_HEADER ClassInstallHeader;
		public DWORD StateChange;
		public DWORD Scope;
		public DWORD HwProfile;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SP_DEVICE_INTERFACE_DATA
	{
		public DWORD cbSize;
		public GUID InterfaceClassGuid;
		public DWORD Flags;
		public ULONG_PTR Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SP_DEVINFO_DATA
	{
		public DWORD cbSize;
		public GUID ClassGuid;
		public DWORD DevInst;
		public ULONG_PTR Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SP_DEVICE_INTERFACE_DETAIL_DATA_W
	{
		public DWORD cbSize;
		public unsafe fixed WCHAR DevicePath[1];
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SECURITY_ATTRIBUTES
	{
		public DWORD nLength;
		public IntPtr lpSecurityDescriptor;
		public BOOL bInheritHandle;
	}
}
