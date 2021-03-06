﻿using System;
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

		/// <summary>
		/// 8 devices max
		/// </summary>
		public Device[] devices { get; private set; }

		public const string lib = "Orbital.Input.DirectInput.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern IntPtr Orbital_Video_DirectInput_Instance_Create();

		[DllImport(lib, CallingConvention = callingConvention)]
		private unsafe static extern int Orbital_Video_DirectInput_Instance_Init(IntPtr handle, IntPtr window, FeatureLevel* minimumFeatureLevel);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern void Orbital_Video_DirectInput_Instance_Dispose(IntPtr handle);

		public Instance()
		{
			handle = Orbital_Video_DirectInput_Instance_Create();
			devices = new Device[8];
			for (int i = 0; i != devices.Length; ++i) devices[i] = new Device(this, i);
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

			// init devices
			for (int i = 0; i != devices.Length; ++i) devices[i].Init();

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
