using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.IO.Ports;
using Orbital.OS.Native;

using DWORD = System.UInt32;
using HDEVINFO = System.IntPtr;
using HWND = System.IntPtr;
using HANDLE = System.IntPtr;
using ULONG = System.UInt32;
using HKEY = System.IntPtr;

namespace Orbital.OS.Serial
{
	public static class SerialUtils
	{
		private static Guid GUID_DEVCLASS_PORTS = new Guid(0x4d36e978u, 0xe325, 0x11ce, 0xbf, 0xc1, 0x08, 0x00, 0x2b, 0xe1, 0x03, 0x18);

		private unsafe static bool GetPortRegistryProperty(HDEVINFO classHandle, SP_DEVINFO_DATA* deviceInfo, uint spdrp, out string result)
		{
			DWORD size;
			SetupAPI.SetupDiGetDeviceRegistryPropertyW(classHandle, deviceInfo, spdrp, null, null, 0, &size);
			if (size == 0)
			{
				result = null;
				return false;
			}

			var resultBuffer = new byte[(int)size];
			fixed (byte* resultBufferPtr = resultBuffer)
			{
				if (SetupAPI.SetupDiGetDeviceRegistryPropertyW(classHandle, deviceInfo, spdrp, null, resultBufferPtr, size, null))
				{
					result = Encoding.Unicode.GetString(resultBufferPtr, (int)size - sizeof(char));
					return true;
				}
				else
				{
					result = null;
					return false;
				}
			}
		}

		/// <summary>
		/// Gathers a list of all active serial port devices
		/// </summary>
		public unsafe static List<SerialDeviceDesc> GetSerialPortDevices()
		{
			var results = new List<SerialDeviceDesc>();

			// get present ports handle
			var classHandle = SetupAPI.SetupDiGetClassDevsW(ref GUID_DEVCLASS_PORTS, null, IntPtr.Zero, SetupAPI.DIGCF_PRESENT);
			if (classHandle == Common.INVALID_HANDLE_VALUE || classHandle == HDEVINFO.Zero) throw new Exception("SetupDiGetClassDevsW failed");

			// enumerate all ports
			var deviceInfo = new SP_DEVINFO_DATA();
			uint deviceInfoSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>();
			deviceInfo.cbSize = deviceInfoSize;
			uint index = 0;
			while (SetupAPI.SetupDiEnumDeviceInfo(classHandle, index, &deviceInfo))
			{
				// get port name
				string portName;
				HKEY regKey = SetupAPI.SetupDiOpenDevRegKey(classHandle, &deviceInfo, SetupAPI.DICS_FLAG_GLOBAL, 0, SetupAPI.DIREG_DEV, WinNT.KEY_READ);
				if (regKey == Common.INVALID_HANDLE_VALUE || regKey == IntPtr.Zero) continue;
				using (var regHandle = new SafeRegistryHandle(regKey, true))
				using (var key = RegistryKey.FromHandle(regHandle))
				{
					portName = key.GetValue("PortName") as string;
					if (string.IsNullOrEmpty(portName)) continue;
				}

				// get registry values
				if (!GetPortRegistryProperty(classHandle, &deviceInfo, SetupAPI.SPDRP_FRIENDLYNAME, out string friendlyName)) continue;
				if (!GetPortRegistryProperty(classHandle, &deviceInfo, SetupAPI.SPDRP_HARDWAREID, out string hardwareID)) continue;

				// add device
				results.Add(new SerialDeviceDesc(friendlyName, portName, hardwareID));

				// setup for next device
				++index;
				deviceInfo = new SP_DEVINFO_DATA();
				deviceInfo.cbSize = deviceInfoSize;
			}

			// finish
			SetupAPI.SetupDiDestroyDeviceInfoList(classHandle);
			return results;
		}

		// <summary>
		/// Gathers a list of active serial port devices matching any of the vid/pid in the device list arg
		/// </summary>
		public static List<SerialDeviceDesc> GetSerialPortDevices(List<SerialDeviceConnectDesc> devices)
		{
			var results = new List<SerialDeviceDesc>();
			foreach (var device in GetSerialPortDevices())
			foreach (var searchDevice in devices)
			{
				if (device.vid == searchDevice.vid && device.pid == searchDevice.pid)
				{
					results.Add(device);
				}
			}
			return results;
		}

		// <summary>
		/// Gathers a list of active serial port devices matching vid/pid
		/// </summary>
		public static List<SerialDeviceDesc> GetSerialPortDevices(ushort vid, ushort pid)
		{
			var devices = new List<SerialDeviceConnectDesc>() {new SerialDeviceConnectDesc(vid, pid)};
			return GetSerialPortDevices(devices);
		}
	}
}
