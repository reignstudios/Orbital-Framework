using System;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;
using HRESULT = System.Int32;
using HMODULE = System.IntPtr;
using HINSTANCE = System.IntPtr;

namespace Orbital.Input.DirectInput
{
	public enum InstanceVersion
	{
		DI_8,
		DI_Legacy
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib_8 = "Dinput8.dll";
		public const string lib_Legacy = "Dinput.dll";
		public const CallingConvention callingConvention = CallingConvention.StdCall;

		[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern HMODULE LoadLibraryW(char* lpLibFileName);

		[DllImport("Ole32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern HRESULT CoInitializeEx(void* pvReserved, DWORD dwCoInit);

		[DllImport("Ole32.dll", CallingConvention = CallingConvention.StdCall)]
		private unsafe static extern HRESULT CoCreateInstance(Guid* rclsid, IntPtr pUnkOuter, DWORD dwClsContext, Guid* riid, void** ppv);

		[DllImport(Instance.lib_8, CallingConvention = callingConvention, EntryPoint = "DirectInput8Create")]
		private unsafe static extern HRESULT DirectInput8Create(HINSTANCE hinst, DWORD dwVersion, Guid* riidltf, void* ppvOut, IntPtr punkOuter);

		public InstanceVersion version { get; private set; }

		public unsafe bool Init()
		{
			// test for v8
			fixed (char* libName = lib_8)
			{
				var library = LoadLibraryW(libName);
				if (library != HMODULE.Zero)
				{
					//HRESULT result = DirectInput8Create();
					version = InstanceVersion.DI_8;
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
