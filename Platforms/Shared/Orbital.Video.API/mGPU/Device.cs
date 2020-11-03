﻿using Orbital.Host;
using System;
using System.IO;

namespace Orbital.Video.API.mGPU
{
	public enum MGPUDeviceType
	{
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

	public struct DeviceDesc
	{
		public D3D12.DeviceDesc descD3D12;

		/// <summary>
		/// Any vendors in this list will be ignored when intializing mGPU mixed-devices
		/// </summary>
		public AdapterVendor[] vendorIgnores_MixedDevices;
	}

	public sealed class Device : DeviceBase
	{
		public DeviceBase[] devices { get; private set; }

		/// <summary>
		/// This device(s) mGPU type after 'Init' is called
		/// </summary>
		public MGPUDeviceType mgpuType { get; private set; }

		/// <summary>
		/// For AFR this is the active device index
		/// </summary>
		public int activeDeviceIndex { get; private set; }

		public DeviceBase activeDevice { get; private set; }

		public Device(InstanceBase instance, DeviceType type, MGPUDeviceType mgpuType)
		: base(instance, type)
		{
			this.mgpuType = mgpuType;
		}

		private static bool IsVendorIgnored(AdapterVendor vender, AdapterVendor[] ignoredVendors)
		{
			foreach (var v in ignoredVendors)
			{
				if (vender == v) return true;
			}
			return false;
		}

		public bool Init(DeviceDesc desc)
		{
			AdapterInfo[] adapters = null;
			bool createSingleDevice = false;
			if (mgpuType == MGPUDeviceType.SingleGPU_Standard)
			{
				mgpuType = MGPUDeviceType.SingleGPU_Standard;
				createSingleDevice = true;// single gpu mode only
			}
			else if (mgpuType == MGPUDeviceType.MultiGPU_BestAvaliable_AFR)
			{
				if (!instance.QuerySupportedAdapters(false, out adapters)) throw new Exception("Failed to get supported adapters");
				if (adapters.Length >= 1)
				{
					// test for linked-gpus
					foreach (var adapter in adapters)
					{
						if (adapter.isPrimary && adapter.nodeCount > 1)
						{
							mgpuType = MGPUDeviceType.MultiGPU_LinkedNode_AFR;
							break;
						}
					}

					// test for mixed-gpu support
					if (mgpuType == MGPUDeviceType.MultiGPU_BestAvaliable_AFR)
					{
						if (adapters.Length >= 2)
						{
							foreach (var adapter in adapters)
							{
								if (adapter.isPrimary)
								{
									mgpuType = MGPUDeviceType.MultiGPU_MixedDevice_AFR;
									break;
								}
							}
						}
					}
				}

				// set to single gpu if no mGPU support found
				if (mgpuType == MGPUDeviceType.MultiGPU_BestAvaliable_AFR)
				{
					mgpuType = MGPUDeviceType.SingleGPU_Standard;
					createSingleDevice = true;
				}
			}

			if (mgpuType == MGPUDeviceType.MultiGPU_LinkedNode_AFR)
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

				if (!linkedNodesFound)
				{
					mgpuType = MGPUDeviceType.SingleGPU_Standard;// default to single gpu mode if only adapter-node found
					createSingleDevice = true;
				}
			}
			else if (mgpuType == MGPUDeviceType.MultiGPU_MixedDevice_AFR)
			{
				if (adapters == null && !instance.QuerySupportedAdapters(false, out adapters)) throw new Exception("Failed to get supported adapters");
				if (adapters.Length > 1)
				{
					// calculate how many valid device we have
					int deviceCount;
					if (desc.vendorIgnores_MixedDevices != null)
					{
						deviceCount = 0;
						foreach (var adapter in adapters)
						{
							if (!IsVendorIgnored(adapter.vendor, desc.vendorIgnores_MixedDevices)) ++deviceCount;
						}
					}
					else
					{
						deviceCount = adapters.Length;
					}

					// create mixed device arrays
					devices = new DeviceBase[deviceCount];// mixed-device mode needs to create a device per physical GPU
					int i = 0;
					foreach (var adapter in adapters)
					{
						if (IsVendorIgnored(adapter.vendor, desc.vendorIgnores_MixedDevices)) continue;

						if (instance is D3D12.Instance) devices[i] = new D3D12.Device((D3D12.Instance)instance, i == 0 ? type : DeviceType.Background);
						else throw new NotImplementedException("Failed to create devices based on instance type: " + instance.GetType().ToString());
						++i;
					}
				}
				else
				{
					mgpuType = MGPUDeviceType.SingleGPU_Standard;// default to single gpu mode if only adapter found
					createSingleDevice = true;
				}
			}

			if (createSingleDevice)
			{
				devices = new DeviceBase[1];
				if (instance is D3D12.Instance) devices[0] = new D3D12.Device((D3D12.Instance)instance, type);
			}

			// force mixed-device AFR requirements
			if (mgpuType == MGPUDeviceType.MultiGPU_MixedDevice_AFR)
			{
				desc.descD3D12.swapChainType = SwapChainType.SingleGPU_Standard;
				desc.descD3D12.swapChainBufferCount = 1;
			}

			// init devices
			foreach (var device in devices)
			{
				if (device is D3D12.Device)
				{
					var deviceD3D12 = (D3D12.Device)device;
					if (!deviceD3D12.Init(desc.descD3D12)) return false;
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			return true;
		}

		public override void Dispose()
		{
			if (devices != null)
			{
				foreach (var device in devices)
				{
					if (device != null) device.Dispose();
				}
				devices = null;
			}
		}

		public override void BeginFrame()
		{
			activeDevice.BeginFrame();
		}

		public override void EndFrame()
		{
			activeDevice.EndFrame();
		}

		public unsafe override bool GetMaxMSAALevel(TextureFormat format, out MSAALevel msaaLevel)
		{
			return activeDevice.GetMaxMSAALevel(format, out msaaLevel);
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, SwapChainVSyncMode vSyncMode)
		{
			return activeDevice.CreateSwapChain(window, bufferCount, fullscreen, ensureSizeMatchesWindowSize, format, type, vSyncMode);
		}

		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, SwapChainVSyncMode vSyncMode)
		{
			return activeDevice.CreateSwapChain(window, bufferCount, fullscreen, ensureSizeMatchesWindowSize, format, type, stencilUsage, depthStencilFormat, depthStencilMode, vSyncMode);
		}

		public override RasterizeCommandListBase CreateRasterizeCommandList()
		{
			return activeDevice.CreateRasterizeCommandList();
		}

		public override ComputeCommandListBase CreateComputeCommandList()
		{
			return activeDevice.CreateComputeCommandList();
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc)
		{
			return activeDevice.CreateRenderPass(desc);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil)
		{
			return activeDevice.CreateRenderPass(desc, depthStencil);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures)
		{
			return activeDevice.CreateRenderPass(desc, renderTextures);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures, DepthStencilBase depthStencil)
		{
			return activeDevice.CreateRenderPass(desc, renderTextures, depthStencil);
		}

		public override RenderStateBase CreateRenderState(RenderStateDesc desc)
		{
			return activeDevice.CreateRenderState(desc);
		}

		public override ShaderEffectBase CreateShaderEffect(Stream stream, ShaderSamplerAnisotropy anisotropyOverride)
		{
			return activeDevice.CreateShaderEffect(stream, anisotropyOverride);
		}

		public override ShaderEffectBase CreateShaderEffect(ShaderBase vs, ShaderBase ps, ShaderBase hs, ShaderBase ds, ShaderBase gs, ShaderEffectDesc desc, bool disposeShaders)
		{
			return activeDevice.CreateShaderEffect(vs, ps, hs, ds, gs, desc, disposeShaders);
		}

		public override ComputeStateBase CreateComputeState(ComputeStateDesc desc)
		{
			return activeDevice.CreateComputeState(desc);
		}

		public override ComputeShaderBase CreateComputeShader(Stream stream, ComputeShaderDesc desc)
		{
			return activeDevice.CreateComputeShader(stream, desc);
		}

		public override ComputeShaderBase CreateComputeShader(byte[] bytecode, ComputeShaderDesc desc)
		{
			return activeDevice.CreateComputeShader(bytecode, desc);
		}

		public override ComputeShaderBase CreateComputeShader(byte[] bytecode, int offset, int length, ComputeShaderDesc desc)
		{
			return activeDevice.CreateComputeShader(bytecode, offset, length, desc);
		}

		public override VertexBufferBase CreateVertexBuffer(uint vertexCount, uint vertexSize, VertexBufferMode mode)
		{
			return activeDevice.CreateVertexBuffer(vertexCount, vertexSize, mode);
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, VertexBufferMode mode)
		{
			return activeDevice.CreateVertexBuffer<T>(vertices, mode);
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, ushort[] indices, VertexBufferMode mode)
		{
			return activeDevice.CreateVertexBuffer<T>(vertices, indices, mode);
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, uint[] indices, VertexBufferMode mode)
		{
			return activeDevice.CreateVertexBuffer<T>(vertices, indices, mode);
		}

		public override IndexBufferBase CreateIndexBuffer(uint indexCount, IndexBufferSize indexSize, IndexBufferMode mode)
		{
			return activeDevice.CreateIndexBuffer(indexCount, indexSize, mode);
		}

		public override IndexBufferBase CreateIndexBuffer(ushort[] indices, IndexBufferMode mode)
		{
			return activeDevice.CreateIndexBuffer(indices, mode);
		}

		public override IndexBufferBase CreateIndexBuffer(uint[] indices, IndexBufferMode mode)
		{
			return activeDevice.CreateIndexBuffer(indices, mode);
		}

		public override VertexBufferStreamerBase CreateVertexBufferStreamer(VertexBufferStreamLayout layout)
		{
			return activeDevice.CreateVertexBufferStreamer(layout);
		}

		public override ConstantBufferBase CreateConstantBuffer(int size, ConstantBufferMode mode)
		{
			return activeDevice.CreateConstantBuffer(size, mode);
		}

		public override ConstantBufferBase CreateConstantBuffer<T>(ConstantBufferMode mode)
		{
			return activeDevice.CreateConstantBuffer<T>(mode);
		}

		public override ConstantBufferBase CreateConstantBuffer<T>(T initialData, ConstantBufferMode mode)
		{
			return activeDevice.CreateConstantBuffer<T>(initialData, mode);
		}

		public override Texture2DBase CreateTexture2D(int width, int height, TextureFormat format, byte[] data, TextureMode mode, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return activeDevice.CreateTexture2D(width, height, format, data, mode, nodeVisibility);
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return activeDevice.CreateRenderTexture2D(width, height, format, usage, mode, msaaLevel, allowRandomAccess, nodeVisibility);
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return activeDevice.CreateRenderTexture2D(width, height, format, usage, data, mode, nodeVisibility);
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return activeDevice.CreateRenderTexture2D(width, height, format, usage, mode, stencilUsage, depthStencilFormat, depthStencilMode, msaaLevel, allowRandomAccess, nodeVisibility);
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return activeDevice.CreateRenderTexture2D(width, height, format, usage, data, mode, stencilUsage, depthStencilFormat, depthStencilMode, allowRandomAccess, nodeVisibility);
		}

		public override DepthStencilBase CreateDepthStencil(int width, int height, DepthStencilFormat format, StencilUsage stencilUsage, DepthStencilMode mode, MSAALevel msaaLevel)
		{
			return activeDevice.CreateDepthStencil(width, height, format, stencilUsage, mode, msaaLevel);
		}
		#endregion
	}
}
