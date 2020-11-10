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
		D3D12,
		Vulkan
		#endif
	}

	public enum AbstractionInitType
	{
		DontInit,

		/// <summary>
		/// Primary devices used only & Swap-Chain only uses primary GPU regardless of Multi-GPU support
		/// </summary>
		SingleGPU_Standard,

		/// <summary>
		/// Use best option for AFR.
		/// LinkedNode option will be used in favor over MixedDevice if avaliable
		/// </summary>
		MultiGPU_BestAvaliable_AFR,

		/// <summary>
		/// Single linked-adapter will be used & Swap-Chain will init back-buffers on each GPU for AFR rendering.
		/// NOTE: If Linked-GPU/Multi-GPU support isn't avaliable or enabled will default to 'SingleGPU_Standard'
		/// </summary>
		MultiGPU_LinkedNode_AFR,

		/// <summary>
		/// Multiple devices will be used & Swap-Chain will init present-back-buffers on primary GPU & extra GPUs each get a back-buffer to be copied to primary GPU for AFR rendering.
		/// NOTE: If Multi-GPU support isn't avaliable will default to 'SingleGPU_Standard'
		/// </summary>
		MultiGPU_MixedDevice_AFR
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
		/// Type of device to init
		/// </summary>
		public DeviceType deviceType = DeviceType.Presentation;

		/// <summary>
		/// APIs to attempt to Init in order
		/// </summary>
		public AbstractionAPI[] supportedAPIs;

		/// <summary>
		/// Any vendors in this list will be ignored when intializing mGPU mixed-devices
		/// </summary>
		public AdapterVendor[] vendorIgnores_MixedDevices;

		#if WIN32 || WINRT
		public D3D12.InstanceDesc instanceDescD3D12;
		public D3D12.DeviceDesc deviceDescD3D12;
		public string nativeLibPathD3D12;

		public Vulkan.InstanceDesc instanceDescVulkan;
		public Vulkan.DeviceDesc deviceDescVulkan;
		public string nativeLibPathVulkan;
		#endif

		public AbstractionDesc(AbstractionInitType type)
		{
			_type = type;
			if (_type == AbstractionInitType.DontInit) _type = AbstractionInitType.SingleGPU_Standard;
			if (type == AbstractionInitType.DontInit) return;

			// set default apis
			supportedAPIs = new AbstractionAPI[]
			{
				#if WIN32 || WINRT
				AbstractionAPI.D3D12,
				AbstractionAPI.Vulkan
				#endif
			};

			// flag AFR defaults
			bool isAFR = type == AbstractionInitType.MultiGPU_BestAvaliable_AFR || type == AbstractionInitType.MultiGPU_LinkedNode_AFR || type == AbstractionInitType.MultiGPU_MixedDevice_AFR;
			if (isAFR)
			{
				vendorIgnores_MixedDevices = new AdapterVendor[1]
				{
					AdapterVendor.Intel
				};
			}

			// set D3D12 defualts
			#if WIN32 || WINRT
			instanceDescD3D12.minimumFeatureLevel = D3D12.FeatureLevel.Level_11_0;
			deviceDescD3D12.adapterIndex = -1;
			deviceDescD3D12.ensureSwapChainMatchesWindowSize = true;
			deviceDescD3D12.swapChainBufferCount = 2;
			if (type == AbstractionInitType.SingleGPU_Standard)
			{
				deviceDescD3D12.allowMultiGPU = false;
				deviceDescD3D12.swapChainType = SwapChainType.SingleGPU_Standard;
			}
			else if (isAFR)
			{
				deviceDescD3D12.allowMultiGPU = true;
				deviceDescD3D12.swapChainType = SwapChainType.MultiGPU_AFR;
			}
			else
			{
				throw new NotImplementedException(type.ToString());
			}
			deviceDescD3D12.createDepthStencil = true;
			#endif

			// set Vulkan defualts
			#if WIN32 || WINRT
			instanceDescVulkan.minimumFeatureLevel = Vulkan.FeatureLevel.Level_1_0;
			deviceDescVulkan.adapterIndex = -1;
			deviceDescVulkan.ensureSwapChainMatchesWindowSize = true;
			deviceDescVulkan.swapChainBufferCount = 2;
			if (type == AbstractionInitType.SingleGPU_Standard)
			{
				deviceDescVulkan.allowMultiGPU = false;
				deviceDescVulkan.swapChainType = SwapChainType.SingleGPU_Standard;
			}
			else if (isAFR)
			{
				deviceDescVulkan.allowMultiGPU = true;
				deviceDescVulkan.swapChainType = SwapChainType.MultiGPU_AFR;
			}
			else
			{
				throw new NotImplementedException(type.ToString());
			}
			deviceDescVulkan.createDepthStencil = true;
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

		private static bool IsVendorIgnored(AdapterVendor vender, AdapterVendor[] ignoredVendors)
		{
			foreach (var v in ignoredVendors)
			{
				if (vender == v) return true;
			}
			return false;
		}

		private static DeviceBase CreateDevice(AbstractionDesc desc, InstanceBase instance)
		{
			DeviceBase device = null;
			AdapterInfo[] adapters = null;
			bool createSingleDevice = false;
			var initType = desc.type;
			if (initType == AbstractionInitType.SingleGPU_Standard)
			{
				initType = AbstractionInitType.SingleGPU_Standard;
				createSingleDevice = true;// single gpu mode only
			}
			else if (initType == AbstractionInitType.MultiGPU_BestAvaliable_AFR)
			{
				if (!instance.QuerySupportedAdapters(false, out adapters)) throw new Exception("Failed to get supported adapters");
				if (adapters.Length >= 1)
				{
					// test for linked-gpus
					foreach (var adapter in adapters)
					{
						if (adapter.isPrimary && adapter.nodeCount > 1)
						{
							initType = AbstractionInitType.MultiGPU_LinkedNode_AFR;
							break;
						}
					}

					// test for mixed-gpu support
					if (initType == AbstractionInitType.MultiGPU_BestAvaliable_AFR)
					{
						if (adapters.Length >= 2)
						{
							foreach (var adapter in adapters)
							{
								if (adapter.isPrimary)
								{
									initType = AbstractionInitType.MultiGPU_MixedDevice_AFR;
									break;
								}
							}
						}
					}
				}

				// set to single gpu if no mGPU support found
				if (initType == AbstractionInitType.MultiGPU_BestAvaliable_AFR)
				{
					initType = AbstractionInitType.SingleGPU_Standard;
					createSingleDevice = true;
				}
			}

			if (initType == AbstractionInitType.MultiGPU_LinkedNode_AFR)
			{
				if (adapters == null && !instance.QuerySupportedAdapters(false, out adapters)) throw new Exception("Failed to get supported adapters");
				bool linkedNodesFound = false;
				foreach (var adapter in adapters)
				{
					if (adapter.isPrimary && adapter.nodeCount > 1)
					{
						linkedNodesFound = true;
						break;
					}
				}

				if (!linkedNodesFound) initType = AbstractionInitType.SingleGPU_Standard;// default to single gpu mode if only adapter-node found
				createSingleDevice = true;// always create a single device
			}
			else if (initType == AbstractionInitType.MultiGPU_MixedDevice_AFR)
			{
				if (adapters == null && !instance.QuerySupportedAdapters(false, out adapters)) throw new Exception("Failed to get supported adapters");
				if (adapters.Length > 1)
				{
					// gather all supported adapters
					List<AdapterInfo> supportedAdapters;
					if (desc.vendorIgnores_MixedDevices != null)
					{
						supportedAdapters = new List<AdapterInfo>();
						foreach (var adapter in adapters)
						{
							if (!IsVendorIgnored(adapter.vendor, desc.vendorIgnores_MixedDevices)) supportedAdapters.Add(adapter);
						}
					}
					else
					{
						supportedAdapters = new List<AdapterInfo>(adapters);
					}

					device = new mGPU.Device(instance, desc.deviceType, supportedAdapters.ToArray());
				}
				else
				{
					initType = AbstractionInitType.SingleGPU_Standard;// default to single gpu mode if only adapter found
					createSingleDevice = true;
				}
			}

			// force mixed-device AFR requirements
			if (initType == AbstractionInitType.MultiGPU_MixedDevice_AFR)
			{
				desc.deviceDescD3D12.swapChainType = SwapChainType.SingleGPU_Standard;
				desc.deviceDescVulkan.swapChainType = SwapChainType.SingleGPU_Standard;
			}

			// create single device if needed
			if (createSingleDevice)
			{
				if (instance is D3D12.Instance) device = new D3D12.Device((D3D12.Instance)instance, desc.deviceType);
				else if (instance is Vulkan.Instance) device = new Vulkan.Device((Vulkan.Instance)instance, desc.deviceType);
			}

			return device;
		}

		/// <summary>
		/// Initializes first API avaliable to the hardware
		/// NOTE: 'desc' may be modified
		/// </summary>
		public static bool InitFirstAvaliable(AbstractionDesc desc, out InstanceBase instance, out DeviceBase device)
		{
			// validate supported APIs is configured
			if (desc.supportedAPIs == null)
			{
				instance = null;
				device = null;
				return false;
			}

			// try to init each API until we find one supported by this hardware
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
							var deviceBase = CreateDevice(desc, instanceD3D12);
							if (deviceBase is D3D12.Device)
							{
								var deviceD3D12 = (D3D12.Device)deviceBase;
								if (deviceD3D12.Init(desc.deviceDescD3D12))
								{
									instance = instanceD3D12;
									device = deviceD3D12;
									return true;
								}

								deviceD3D12.Dispose();
							}
							else if (deviceBase is mGPU.Device)
							{
								var deviceMGPU = (mGPU.Device)deviceBase;
								if (deviceMGPU.Init(desc))
								{
									instance = instanceD3D12;
									device = deviceMGPU;
									return true;
								}

								deviceMGPU.Dispose();
							}

							instanceD3D12.Dispose();
						}
						else
						{
							instanceD3D12.Dispose();
						}
					}
					break;

					/*case AbstractionAPI.Vulkan:
					{
						if (!LoadNativeLib(Path.Combine(desc.nativeLibPathVulkan, "vulkan-1.dll"))) continue;
						if (!LoadNativeLib(Path.Combine(desc.nativeLibPathVulkan, Vulkan.Instance.lib))) continue;
						var instanceVulkan = new Vulkan.Instance();
						if (instanceVulkan.Init(desc.instanceDescVulkan))
						{
							var deviceVulkan = new Vulkan.Device(instanceVulkan, desc.deviceType);
							if (deviceVulkan.Init(desc.deviceDescVulkan))
							{
								instance = instanceVulkan;
								device = deviceVulkan;
								return true;
							}

							deviceVulkan.Dispose();
							instanceVulkan.Dispose();
						}
						else
						{
							instanceVulkan.Dispose();
						}
					}
					break;*/
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

					case AbstractionAPI.Vulkan:
					{
						if (!LoadNativeLib(Path.Combine(desc.nativeLibPathVulkan, Vulkan.Instance.lib))) continue;
						var instanceVulkan = new Vulkan.Instance();
						if (instanceVulkan.Init(desc.instanceDescVulkan)) instances.Add(instanceVulkan);
						else instanceVulkan.Dispose();
					}
					break;
					#endif
				}
			}

			return true;
		}
	}
}
