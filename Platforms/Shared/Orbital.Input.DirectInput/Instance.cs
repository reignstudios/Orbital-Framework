using Orbital.Primitives;
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

	public sealed partial class Instance : InstanceBase
	{
		internal IntPtr handle;
		public FeatureLevel featureLevel { get; private set; }

		/// <summary>
		/// 8 devices max
		/// </summary>
		public ReadOnlyArray<Device> devicesDI { get; private set; }

		public const string lib = "Orbital.Input.DirectInput.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern IntPtr Orbital_Video_DirectInput_Instance_Create();

		[DllImport(lib, CallingConvention = callingConvention)]
		private unsafe static extern int Orbital_Video_DirectInput_Instance_Init(IntPtr handle, IntPtr window, FeatureLevel* minimumFeatureLevel);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern void Orbital_Video_DirectInput_Instance_Dispose(IntPtr handle);

		public Instance(bool autoConfigureAbstractions)
		: base(autoConfigureAbstractions)
		{
			handle = Orbital_Video_DirectInput_Instance_Create();
			Device[] devices_backing;
			devicesDI = new ReadOnlyArray<Device>(8, out devices_backing);
			for (int i = 0; i != devices_backing.Length; ++i) devices_backing[i] = new Device(this, i);
			devices = new ReadOnlyArray<DeviceBase>(devices_backing);
		}

		public bool Init(FeatureLevel minimumFeatureLevel)
		{
			return Init(IntPtr.Zero, minimumFeatureLevel);
		}

		public unsafe bool Init(IntPtr hwnd, FeatureLevel minimumFeatureLevel)
		{
			FeatureLevel level;
			if (Orbital_Video_DirectInput_Instance_Init(handle, hwnd, &level) == 0) return false;
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

			base.Dispose();
		}
	}
}
