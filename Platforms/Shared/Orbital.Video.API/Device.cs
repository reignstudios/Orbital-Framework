using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Orbital.Video.API
{
	/// <summary>
	/// Native device APIs
	/// </summary>
	public enum DeviceAPI
	{
		#if WIN32 || WINRT
		D3D12
		#endif
	}

	public class DeviceDesc
	{
		/// <summary>
		/// Type of device to init
		/// </summary>
		public DeviceType type = DeviceType.Presentation;

		/// <summary>
		/// APIs to attempt to Init in order
		/// </summary>
		public DeviceAPI[] supportedAPIs;

		#if WIN32 || WINRT
		public D3D12.DeviceDesc descD3D12;
		public string nativeLibPathD3D12;
		#endif

		public DeviceDesc()
		{
			// set default apis
			supportedAPIs = new DeviceAPI[]
			{
				#if WIN32 || WINRT
				DeviceAPI.D3D12
				#endif
			};

			// set D3D12 defualts
			#if WIN32 || WINRT
			descD3D12.adapterIndex = -1;
			descD3D12.ensureSwapChainMatchesWindowSize = true;
			descD3D12.swapChainBufferCount = 2;
			#endif
		}
	}

	public static class Device
	{
		#if WIN32
		[DllImport("Kernel32.dll", EntryPoint = "LoadLibraryA")]
		private static extern unsafe IntPtr LoadLibraryA(byte* lpLibFileName);
		#endif

		private unsafe static void LoadNativeLib(string libPath)
		{
			byte[] libNameEncoded = Encoding.Default.GetBytes(libPath);
			fixed (byte* libNameEncodedPtr = libNameEncoded)
			{
				#if WIN32
				IntPtr lib = LoadLibraryA(libNameEncodedPtr);
				#endif
				if (lib == IntPtr.Zero) throw new Exception("Failed to load lib: " + libPath);
			}
		}

		public static DeviceBase Init(DeviceDesc desc)
		{
			if (desc.supportedAPIs == null) return null;
			DeviceBase device = null;
			foreach (var api in desc.supportedAPIs)
			{
				switch (api)
				{
					#if WIN32 || WIN32
					case DeviceAPI.D3D12:
					{
						LoadNativeLib(Path.Combine(desc.nativeLibPathD3D12, "Orbital.Video.D3D12.Native.dll"));
						var deviceD3D12 = new D3D12.Device(desc.type);
						if (deviceD3D12.Init(desc.descD3D12))
						{
							device = deviceD3D12;
						}
						else
						{
							deviceD3D12.Dispose();
							continue;
						}
						break;
					}
					#endif
				}

				if (device != null) break;
			}

			if (device == null) throw new Exception("Failed to create Device");
			return device;
		}
	}
}
