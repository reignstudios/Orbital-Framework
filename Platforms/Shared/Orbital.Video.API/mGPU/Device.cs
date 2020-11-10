using Orbital.Host;
using System;
using System.IO;

namespace Orbital.Video.API.mGPU
{
	public sealed class Device : DeviceBase
	{
		public DeviceBase[] devices { get; private set; }
		public DeviceBase primaryDevice { get; private set; }

		public int activeDeviceIndex { get; private set; }
		public DeviceBase activeDevice { get; private set; }

		public Device(InstanceBase instance, DeviceType type, AdapterInfo[] adapters)
		: base(instance, type)
		{
			if (adapters == null || adapters.Length <= 1) throw new ArgumentException("Adapters must be at least two in length");
			devices = new DeviceBase[adapters.Length];
			for (int i = 0; i != devices.Length; ++i)
			{
				// allocate API specific device
				if (instance is D3D12.Instance) devices[i] = new D3D12.Device((D3D12.Instance)instance, adapters[i].isPrimary ? type : DeviceType.Background);
				else if (instance is Vulkan.Instance) devices[i] = new Vulkan.Device((Vulkan.Instance)instance, adapters[i].isPrimary ? type : DeviceType.Background);
				else throw new NotImplementedException("Failed to create devices based on instance type: " + instance.GetType().ToString());

				// set primary device
				if (adapters[i].isPrimary) primaryDevice = devices[i];
			}
		}

		public bool Init(AbstractionDesc desc)
		{
			for (int i = 0; i != devices.Length; ++i)
			{
				if (instance is D3D12.Instance)
				{
					var deviceD3D12 = (D3D12.Device)devices[i];
					if (!deviceD3D12.Init(desc.deviceDescD3D12)) return false;
				}
				else if (instance is Vulkan.Instance)
				{
					var deviceVulkan = (Vulkan.Device)devices[i];
					if (!deviceVulkan.Init(desc.deviceDescVulkan)) return false;
				}
				else throw new NotImplementedException("Failed to create devices based on instance type: " + instance.GetType().ToString());
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
			activeDevice = devices[activeDeviceIndex];
			activeDevice.BeginFrame();
		}

		public override void EndFrame()
		{
			activeDevice.EndFrame();
			++activeDeviceIndex;
			if (activeDeviceIndex >= devices.Length) activeDeviceIndex = 0;
		}

		/// <summary>
		/// Will find the MSAA level compatible with all devices
		/// </summary>
		public unsafe override bool GetMaxMSAALevel(TextureFormat format, out MSAALevel msaaLevel)
		{
			msaaLevel = MSAALevel.X16;
			foreach (var device in devices)
			{
				MSAALevel deviceMSAA;
				if (!device.GetMaxMSAALevel(format, out deviceMSAA)) return false;
				if (deviceMSAA < msaaLevel) msaaLevel = deviceMSAA;
			}
			return true;
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, SwapChainVSyncMode vSyncMode)
		{
			return primaryDevice.CreateSwapChain(window, bufferCount, fullscreen, ensureSizeMatchesWindowSize, format, type, vSyncMode);
		}

		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, SwapChainVSyncMode vSyncMode)
		{
			return primaryDevice.CreateSwapChain(window, bufferCount, fullscreen, ensureSizeMatchesWindowSize, format, type, stencilUsage, depthStencilFormat, depthStencilMode, vSyncMode);
		}

		public override RasterizeCommandListBase CreateRasterizeCommandList()
		{
			var abstraction = new RasterizeCommandList(this);
			if (!abstraction.Init())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create CommandList");
			}
			return abstraction;
		}

		public override ComputeCommandListBase CreateComputeCommandList()
		{
			var abstraction = new ComputeCommandList(this);
			if (!abstraction.Init())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create CommandList");
			}
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc)
		{
			return primaryDevice.swapChain.CreateRenderPass(desc);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil)
		{
			return primaryDevice.swapChain.CreateRenderPass(desc, depthStencil);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures)
		{
			var abstraction = new RenderPass(this);
			/*if (!abstraction.Init(desc, (RenderTexture2D[])renderTextures))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderState");
			}*/
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures, DepthStencilBase depthStencil)
		{
			var abstraction = new RenderPass(this);
			/*if (!abstraction.Init(desc, (RenderTexture2D[])renderTextures, (DepthStencil)depthStencil))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderState");
			}*/
			return abstraction;
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
