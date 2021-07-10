using Orbital.Primitives;
using System.Runtime.InteropServices;
using System.Text;

using HMODULE = System.IntPtr;
using FARPROC = System.IntPtr;
using DWORD = System.UInt32;
using WORD = System.UInt16;

namespace Orbital.Input.XInput
{
	public enum InstanceVersion
	{
		/// <summary>
		/// Legacy version
		/// </summary>
		XInput_1_1,

		/// <summary>
		/// Legacy version
		/// </summary>
		XInput_1_2,

		/// <summary>
		/// Support started with Windows XP
		/// </summary>
		XInput_1_3,

		/// <summary>
		/// Support started with Windows 8
		/// </summary>
		XInput_1_4,

		/// <summary>
		/// Limited feature lib starting in Vista for broad compatibility
		/// </summary>
		XInput_9_1_0,
	}

	public sealed partial class Instance : InstanceBase
	{
		public const string lib_1_1 = "xinput1_1.dll";
		public const string lib_1_2 = "xinput1_2.dll";
		public const string lib_1_3 = "xinput1_3.dll";
		public const string lib_1_4 = "xinput1_4.dll";
		public const string lib_9_1_0 = "XInput9_1_0.dll";

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern HMODULE LoadLibraryW(char* lpLibFileName);

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern FARPROC GetProcAddress(HMODULE hModule, byte* lpProcName);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		internal unsafe delegate DWORD XInputGetState_Method(DWORD dwUserIndex, XINPUT_STATE* pState);
		internal XInputGetState_Method XInputGetState;

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		internal unsafe delegate DWORD XInputSetState_Method(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);
		internal XInputSetState_Method XInputSetState;

		public InstanceVersion version { get; private set; }

		/// <summary>
		/// 4 devices max
		/// </summary>
		public ReadOnlyArray<Device> devicesXI { get; private set; }

		public Instance(bool autoConfigureAbstractions)
		: base(autoConfigureAbstractions)
		{
			Device[] devices_backing;
			devicesXI = new ReadOnlyArray<Device>(4, out devices_backing);
			for (int i = 0; i != devices_backing.Length; ++i) devices_backing[i] = new Device(this, i);
			devices = new ReadOnlyArray<DeviceBase>(devices_backing);
		}

		public unsafe bool Init()
		{
			// test for v1.4
			fixed (char* libName = lib_1_4)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					version = InstanceVersion.XInput_1_4;
					return LoadMethods(library);
				}
			}

			// test for v1.3
			fixed (char* libName = lib_1_3)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					version = InstanceVersion.XInput_1_3;
					return LoadMethods(library);
				}
			}

			// test for v1.2
			fixed (char* libName = lib_1_2)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					version = InstanceVersion.XInput_1_2;
					return LoadMethods(library);
				}
			}

			// test for v1.1
			fixed (char* libName = lib_1_1)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					version = InstanceVersion.XInput_1_1;
					return LoadMethods(library);
				}
			}

			// test for v9.1.0
			fixed (char* libName = lib_9_1_0)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					version = InstanceVersion.XInput_9_1_0;
					return LoadMethods(library);
				}
			}

			return false;
		}

		private unsafe bool LoadMethods(HMODULE library)
		{
			FARPROC address;
			byte[] name;

			name = Encoding.ASCII.GetBytes("XInputGetState");
			fixed (byte* namePtr = name) address = GetProcAddress(library, namePtr);
			if (address == FARPROC.Zero) return false;
			XInputGetState = Marshal.GetDelegateForFunctionPointer<XInputGetState_Method>(address);

			name = Encoding.ASCII.GetBytes("XInputSetState");
			fixed (byte* namePtr = name) address = GetProcAddress(library, namePtr);
			if (address == FARPROC.Zero) return false;
			XInputSetState = Marshal.GetDelegateForFunctionPointer<XInputSetState_Method>(address);

			return true;
		}
	}
}
