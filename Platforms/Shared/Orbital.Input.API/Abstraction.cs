﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Orbital.Input.API
{
	/// <summary>
	/// Native device APIs
	/// </summary>
	public enum AbstractionAPI
	{
		#if WIN || WINRT
		WindowsGamingInput,
		XInput,
		DirectInput,
		HID
		#elif MAC
		Mac,
		Cocoa,
		HID
		#endif
	}

	public enum AbstractionInitType
	{
		DontInit,

		/// <summary>
		/// Only a single native abstraction API will be init
		/// </summary>
		SingleAPI,

		/// <summary>
		/// Init multi-api abstraction if avaliable. If platform only supports one 'SingleAPI' will be used instead
		/// </summary>
		MultiAPI
	}

	[Flags]
	public enum AbstractionInputRequirments
	{
		/// <summary>
		/// No requirements
		/// </summary>
		None = 0,

		/// <summary>
		/// API must support Gamepads
		/// </summary>
		Gamepad = 1,

		/// <summary>
		/// API must support Gamepads
		/// </summary>
		ArcadeStick = 2,

		/// <summary>
		/// API must support Flightsicks
		/// </summary>
		FlightStick = 4,

		/// <summary>
		/// API must support Steering-Wheels
		/// </summary>
		SteeringWheel = 8,

		/// <summary>
		/// API must support all input hardware types
		/// </summary>
		All = Gamepad | ArcadeStick | FlightStick | SteeringWheel
	}

	public class AbstractionDesc
	{
		/// <summary>
		/// How the abstractions should try to init
		/// </summary>
		public AbstractionInitType type
		{
			get => _type;
			set
			{
				if (value == AbstractionInitType.DontInit) throw new NotSupportedException("'DontInit' can only be used in constructor");
				_type = value;
			}
		}
		private AbstractionInitType _type;

		/// <summary>
		/// APIs to attempt to Init in order
		/// </summary>
		public AbstractionAPI[] supportedAPIs;

		/// <summary>
		/// API input flag requirments
		/// </summary>
		public AbstractionInputRequirments inputRequirments;

		/// <summary>
		/// Disables XInput devices on non-XInput APIs
		/// </summary>
		public bool ignoreXInputDevices;

		/// <summary>
		/// Auto configure device abstractions such as 'gamepads' etc
		/// </summary>
		public bool autoConfigureAbstractions;

		#if WIN || WINRT
		public string nativeLibPathDirectInput;
		#endif

		public AbstractionDesc(AbstractionInitType type)
		{
			_type = type;
			if (_type == AbstractionInitType.DontInit) _type = AbstractionInitType.MultiAPI;
			if (type == AbstractionInitType.DontInit) return;

			// set default apis
			supportedAPIs = new AbstractionAPI[]
			{
				#if WIN || WINRT
				AbstractionAPI.WindowsGamingInput,
				AbstractionAPI.XInput,
				AbstractionAPI.DirectInput
				#elif MAC
				AbstractionAPI.Mac,
				AbstractionAPI.Cocoa
				#endif
			};

			// disable XInput devices on non-XInput APIs
			ignoreXInputDevices = true;

			// auto init device abstractions
			autoConfigureAbstractions = true;
		}
	}

	public static class Abstraction
	{
		#if WIN
		[DllImport("Kernel32.dll", EntryPoint = "LoadLibraryA")]
		private static extern unsafe IntPtr LoadLibraryA(byte* lpLibFileName);
		#endif

		private unsafe static bool LoadNativeLib(string libPath)
		{
			byte[] libNameEncoded = Encoding.Default.GetBytes(libPath);
			fixed (byte* libNameEncodedPtr = libNameEncoded)
			{
				IntPtr lib;
				#if WIN
				lib = LoadLibraryA(libNameEncodedPtr);
				#else
				lib = IntPtr.Zero;
				#endif
				return lib != IntPtr.Zero;
			}
		}

		/// <summary>
		/// Initializes first API avaliable to the hardware
		/// NOTE: 'desc' may be modified
		/// </summary>
		public static bool InitFirstAvaliable(AbstractionDesc desc, out InstanceBase instance)
		{
			// validate supported APIs is configured
			if (desc.supportedAPIs == null)
			{
				instance = null;
				return false;
			}

			// try to init each API until we find one supported by this hardware
			foreach (var api in desc.supportedAPIs)
			{
				switch (api)
				{
					#if WIN || WINRT
					case AbstractionAPI.WindowsGamingInput:
					{
						throw new NotImplementedException();
					}
					break;

					case AbstractionAPI.XInput:
					{
						var instanceXInput = new XInput.Instance(desc.autoConfigureAbstractions);
						if (instanceXInput.Init())
						{
							instance = instanceXInput;
							return true;
						}
						else
						{
							instanceXInput.Dispose();
						}
					}
					break;

					case AbstractionAPI.DirectInput:
					{
						if (!LoadNativeLib(Path.Combine(desc.nativeLibPathDirectInput, DirectInput.Instance.lib))) continue;
						var instanceXInput = new DirectInput.Instance(desc.ignoreXInputDevices, desc.autoConfigureAbstractions);
						if (instanceXInput.Init(IntPtr.Zero, DirectInput.FeatureLevel.Level_1))
						{
							instance = instanceXInput;
							return true;
						}
						else
						{
							instanceXInput.Dispose();
						}
					}
					break;
					#elif MAC
					case AbstractionAPI.Mac:
					{
						throw new NotImplementedException();
					}
					break;
					
					case AbstractionAPI.Cocoa:
					{
						throw new NotImplementedException();
					}
					break;
					#endif

					case AbstractionAPI.HID:
					{
						throw new NotImplementedException();
					}
					break;
				}
			}

			instance = null;
			return false;
		}
	}
}
