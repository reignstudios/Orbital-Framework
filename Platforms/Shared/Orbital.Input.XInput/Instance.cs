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
	}
}
