﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.Vulkan
{
	public struct DeviceDesc
	{
		/// <summary>
		/// Represents "physical device group" index if Vulkan API 1.1 or newer. Otherwise "physical device" index
		/// </summary>
		public int adapterIndex;

		/// <summary>
		/// Allow the use of multiple GPUs that are linked when avaliable. Resources will be duplicated on all linked GPUs
		/// </summary>
		public bool allowImplicitMultiGPU;

		/// <summary>
		/// Window to the device will present to. Can be null for background devices
		/// </summary>
		public WindowBase window;

		/// <summary>
		/// If the window size changes, auto resize the swap-chain to match
		/// </summary>
		public bool ensureSwapChainMatchesWindowSize;

		/// <summary>
		/// Double/Tripple buffering etc. (NOTE: ignored if multi-gpu rendering is active & avaliable)
		/// </summary>
		public int swapChainBufferCount;

		/// <summary>
		/// Surface format of the swap-chain
		/// </summary>
		public SwapChainFormat swapChainFormat;

		/// <summary>
		/// Type of swap-chain
		/// </summary>
		public SwapChainType swapChainType;

		/// <summary>
		/// True to launch in fullscreen
		/// </summary>
		public bool fullscreen;

		/// <summary>
		/// True to create a depth-buffer managed by the swap-chain 
		/// </summary>
		public bool createDepthStencil;

		/// <summary>
		/// Depth-Stencil stencil specific usage
		/// </summary>
		public StencilUsage stencilUsage;

		/// <summary>
		/// Depth-Stencil format if created
		/// </summary>
		public DepthStencilFormat depthStencilFormat;
		
		/// <summary>
		/// Depth-Stencil mode if created
		/// </summary>
		public DepthStencilMode depthStencilMode;
	}

	public sealed class Device : DeviceBase
	{
		public readonly Instance instanceVulkan;
		internal IntPtr handle;
		public SwapChain swapChainVulkan { get; private set; }

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_Device_Create(IntPtr Instance, DeviceType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_Vulkan_Device_Init(IntPtr handle, int adapterIndex);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_EndFrame(IntPtr handle);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceVulkan = instance;
			handle = Orbital_Video_Vulkan_Device_Create(instance.handle, type);
		}

		public bool Init(DeviceDesc desc)
		{
			if (Orbital_Video_Vulkan_Device_Init(handle, desc.adapterIndex) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChainVulkan = new SwapChain(this, desc.ensureSwapChainMatchesWindowSize, desc.swapChainType);
				swapChain = swapChainVulkan;
				return swapChainVulkan.Init(desc.window, desc.swapChainBufferCount, desc.fullscreen);
			}
			else
			{
				return true;
			}
		}

		public override void Dispose()
		{
			swapChain = null;
			if (swapChainVulkan != null)
			{
				swapChainVulkan.Dispose();
				swapChainVulkan = null;
			}

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			Orbital_Video_Vulkan_Device_BeginFrame(handle);
			if (type == DeviceType.Presentation) swapChainVulkan.BeginFrame();
		}

		public override void EndFrame()
		{
			if (type == DeviceType.Presentation) swapChainVulkan.Present();
			Orbital_Video_Vulkan_Device_EndFrame(handle);
		}

		public override bool GetMaxMSAALevel(TextureFormat format, out MSAALevel msaaLevel)
		{
			throw new NotImplementedException();
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, SwapChainVSyncMode vSyncMode)
		{
			var abstraction = new SwapChain(this, ensureSizeMatchesWindowSize, type);
			if (!abstraction.Init(window, bufferCount, fullscreen))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
		}

		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, SwapChainVSyncMode vSyncMode)
		{
			throw new NotImplementedException();
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
			return swapChainVulkan.CreateRenderPass(desc);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil)
		{
			throw new NotImplementedException();
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures)
		{
			throw new NotImplementedException();
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures, DepthStencilBase depthStencil)
		{
			throw new NotImplementedException();
		}

		public override RenderStateBase CreateRenderState(RenderStateDesc desc)
		{
			throw new NotImplementedException();
		}

		public override ShaderEffectBase CreateShaderEffect(Stream stream, ShaderSamplerAnisotropy anisotropyOverride)
		{
			throw new NotImplementedException();
		}

		public override ShaderEffectBase CreateShaderEffect(ShaderBase vs, ShaderBase ps, ShaderBase hs, ShaderBase ds, ShaderBase gs, ShaderEffectDesc desc, bool disposeShaders)
		{
			throw new NotImplementedException();
		}

		public override ComputeStateBase CreateComputeState(ComputeStateDesc desc)
		{
			throw new NotImplementedException();
		}

		public override ComputeShaderBase CreateComputeShader(Stream stream, ComputeShaderDesc desc)
		{
			throw new NotImplementedException();
		}

		public override ComputeShaderBase CreateComputeShader(byte[] bytecode, ComputeShaderDesc desc)
		{
			throw new NotImplementedException();
		}

		public override ComputeShaderBase CreateComputeShader(byte[] bytecode, int offset, int length, ComputeShaderDesc desc)
		{
			throw new NotImplementedException();
		}

		public override VertexBufferBase CreateVertexBuffer(uint vertexCount, uint vertexSize, VertexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, VertexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, ushort[] indices, VertexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, uint[] indices, VertexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override IndexBufferBase CreateIndexBuffer(uint indexCount, IndexBufferSize indexSize, IndexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override IndexBufferBase CreateIndexBuffer(ushort[] indices, IndexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override IndexBufferBase CreateIndexBuffer(uint[] indices, IndexBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override VertexBufferStreamerBase CreateVertexBufferStreamer(VertexBufferStreamLayout layout)
		{
			throw new NotImplementedException();
		}

		public override ConstantBufferBase CreateConstantBuffer(int size, ConstantBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override ConstantBufferBase CreateConstantBuffer<T>(ConstantBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override ConstantBufferBase CreateConstantBuffer<T>(T initialData, ConstantBufferMode mode)
		{
			throw new NotImplementedException();
		}

		public override Texture2DBase CreateTexture2D(int width, int height, TextureFormat format, byte[] data, TextureMode mode, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			throw new NotImplementedException();
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			throw new NotImplementedException();
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			throw new NotImplementedException();
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			throw new NotImplementedException();
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			throw new NotImplementedException();
		}

		public override DepthStencilBase CreateDepthStencil(int width, int height, DepthStencilFormat format, StencilUsage stencilUsage, DepthStencilMode mode, MSAALevel msaaLevel)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
