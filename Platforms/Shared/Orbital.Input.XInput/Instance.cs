using Orbital.Primitives;
using System.Runtime.InteropServices;

using HMODULE = System.IntPtr;

namespace Orbital.Input.XInput
{
	public enum InstanceVersion
	{
		XInput_1_1,
		XInput_1_2,
		XInput_1_3,
		XInput_1_4,
		XInput9_1_0,
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib_1_4 = "xinput1_4.dll";// TODO: add support for more lib versions
		public const CallingConvention callingConvention = CallingConvention.StdCall;

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern HMODULE LoadLibraryW(char* lpLibFileName);

		public InstanceVersion version { get; private set; }

		/// <summary>
		/// 4 devices max
		/// </summary>
		public ReadOnlyArray<Device> devicesXI { get; private set; }

		public Instance()
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
					return true;
				}
			}

			return false;
		}
	}
}
