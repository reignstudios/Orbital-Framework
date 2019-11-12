using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Orbital.Video.API
{
	/// <summary>
	/// Native device APIs
	/// </summary>
	public enum AbstractionAPI
	{
		#if WIN32 || WINRT
		D3D12
		#endif
	}

	public class AbstractionDesc
	{
		/// <summary>
		/// Type of device to init
		/// </summary>
		public DeviceType type = DeviceType.Presentation;

		/// <summary>
		/// APIs to attempt to Init in order
		/// </summary>
		public AbstractionAPI[] supportedAPIs;

		#if WIN32 || WINRT
		public D3D12.InstanceDesc instanceDescD3D12;
		public D3D12.DeviceDesc deviceDescD3D12;
		public string nativeLibPathD3D12;
		#endif

		public AbstractionDesc(bool initDefaults)
		{
			if (!initDefaults) return;

			// set default apis
			supportedAPIs = new AbstractionAPI[]
			{
				#if WIN32 || WINRT
				AbstractionAPI.D3D12
				#endif
			};

			// set D3D12 defualts
			#if WIN32 || WINRT
			instanceDescD3D12.minimumFeatureLevel = D3D12.FeatureLevel.Level_11_0;
			deviceDescD3D12.adapterIndex = -1;
			deviceDescD3D12.ensureSwapChainMatchesWindowSize = true;
			deviceDescD3D12.swapChainBufferCount = 2;
			#endif
		}
	}

	public static class Abstraction
	{
		#if WIN32
		[DllImport("Kernel32.dll", EntryPoint = "LoadLibraryA")]
		private static extern unsafe IntPtr LoadLibraryA(byte* lpLibFileName);
		#endif

		private unsafe static bool LoadNativeLib(string libPath)
		{
			byte[] libNameEncoded = Encoding.Default.GetBytes(libPath);
			fixed (byte* libNameEncodedPtr = libNameEncoded)
			{
				#if WIN32
				IntPtr lib = LoadLibraryA(libNameEncodedPtr);
				#endif
				return lib != IntPtr.Zero;
			}
		}

		public static bool InitFirstAvaliable(AbstractionDesc desc, out InstanceBase instance, out DeviceBase device)
		{
			if (desc.supportedAPIs == null)
			{
				instance = null;
				device = null;
				return false;
			}
			
			foreach (var api in desc.supportedAPIs)
			{
				switch (api)
				{
					#if WIN32 || WIN32
					case AbstractionAPI.D3D12:
					{
						if (!LoadNativeLib(Path.Combine(desc.nativeLibPathD3D12, D3D12.Instance.lib))) continue;
						var instanceD3D12 = new D3D12.Instance();
						if (instanceD3D12.Init(desc.instanceDescD3D12))
						{
							var deviceD3D12 = new D3D12.Device(instanceD3D12, desc.type);
							if (deviceD3D12.Init(desc.deviceDescD3D12))
							{
								instance = instanceD3D12;
								device = deviceD3D12;
								return true;
							}

							deviceD3D12.Dispose();
							instanceD3D12.Dispose();
						}
						else
						{
							instanceD3D12.Dispose();
						}
					}
					break;
					#endif
				}
			}

			instance = null;
			device = null;
			return false;
		}

		public static bool InitAllAvaliableInstances(AbstractionDesc desc, out List<InstanceBase> instances)
		{
			instances = new List<InstanceBase>();
			if (desc.supportedAPIs == null) return false;
			
			foreach (var api in desc.supportedAPIs)
			{
				switch (api)
				{
					#if WIN32 || WIN32
					case AbstractionAPI.D3D12:
					{
						if (!LoadNativeLib(Path.Combine(desc.nativeLibPathD3D12, D3D12.Instance.lib))) continue;
						var instanceD3D12 = new D3D12.Instance();
						if (instanceD3D12.Init(desc.instanceDescD3D12)) instances.Add(instanceD3D12);
						else instanceD3D12.Dispose();
					}
					break;
					#endif
				}
			}

			return true;
		}
	}
}
