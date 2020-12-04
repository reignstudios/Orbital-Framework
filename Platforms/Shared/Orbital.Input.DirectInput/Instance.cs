using System;
using System.Runtime.InteropServices;

namespace Orbital.Input.DirectInput
{
	public enum FeatureLevel
	{
		Level_1,
		Level_2,
		Level_7,
		Level_8
	}

	public sealed class Instance : InstanceBase
	{
		internal IntPtr handle;
		public FeatureLevel featureLevel { get; private set; }

		public const string lib = "Orbital.Input.DirectInput.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern IntPtr Orbital_Video_DirectInput_Instance_Create();

		[DllImport(lib, CallingConvention = callingConvention)]
		private unsafe static extern int Orbital_Video_DirectInput_Instance_Init(IntPtr handle, FeatureLevel* minimumFeatureLevel);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern void Orbital_Video_DirectInput_Instance_Dispose(IntPtr handle);

		public Instance()
		{
			handle = Orbital_Video_DirectInput_Instance_Create();
		}

		public unsafe bool Init(FeatureLevel minimumFeatureLevel)
		{
			FeatureLevel level;
			if (Orbital_Video_DirectInput_Instance_Init(handle, &level) == 0) return false;
			featureLevel = level;
			if (level < minimumFeatureLevel) return false;
			return true;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_DirectInput_Instance_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
