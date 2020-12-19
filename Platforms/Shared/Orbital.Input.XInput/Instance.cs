using System.Runtime.InteropServices;

using HMODULE = System.IntPtr;

namespace Orbital.Input.XInput
{
	public enum InstanceVersion
	{
		XInput_1_3
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib_1_3 = "xinput1_3.dll";// TODO: add support for more lib versions
		public const CallingConvention callingConvention = CallingConvention.StdCall;

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern HMODULE LoadLibraryW(char* lpLibFileName);

		public InstanceVersion version { get; private set; }

		/// <summary>
		/// 4 devices max
		/// </summary>
		public Device[] devices { get; private set; }

		public Instance()
		{
			devices = new Device[4];
			for (int i = 0; i != devices.Length; ++i) devices[i] = new Device(this, i);
		}

		public unsafe bool Init()
		{
			// test for v1.3
			fixed (char* libName = lib_1_3)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					version = InstanceVersion.XInput_1_3;
					return true;
				}
			}

			return false;
		}

		public override void Dispose()
		{
			// do nothing...
		}

		public override void Update()
		{
			foreach (var device in devices)
			{
				device.Update();
			}
		}

		public override DeviceBase[] GetDevices()
		{
			return devices;
		}
	}
}
