using System;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.D3D12
{
	public struct DeviceDesc
	{
		/// <summary>
		/// Represents physical device index
		/// </summary>
		public int adapterIndex;

		/// <summary>
		/// True if you want to create a WARP device
		/// </summary>
		public bool softwareRasterizer;

		/// <summary>
		/// Window to the device will present to. Can be null for background devices
		/// </summary>
		public WindowBase window;

		/// <summary>
		/// If the window size changes, auto resize the swap-chain to match
		/// </summary>
		public bool ensureSwapChainMatchesWindowSize;

		/// <summary>
		/// Double/Tripple buffering etc
		/// </summary>
		public int swapChainBufferCount;

		/// <summary>
		/// Surface format of the swap-chain
		/// </summary>
		public SwapChainFormat swapChainFormat;

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
		public readonly Instance instanceD3D12;
		internal IntPtr handle;
		public SwapChain swapChainD3D12 { get; private set; }

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create(IntPtr Instance, DeviceType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, int softwareRasterizer);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_EndFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern int Orbital_Video_D3D12_Device_GetMaxMSAALevel(IntPtr handle, TextureFormat format, MSAALevel* msaaLevel);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceD3D12 = instance;
			handle = Orbital_Video_D3D12_Device_Create(instance.handle, type);
		}

		public bool Init(DeviceDesc desc)
		{
			if (Orbital_Video_D3D12_Device_Init(handle, desc.adapterIndex, (desc.softwareRasterizer ? 1 : 0)) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChainD3D12 = new SwapChain(this, desc.ensureSwapChainMatchesWindowSize);
				swapChain = swapChainD3D12;
				if (desc.createDepthStencil) return swapChainD3D12.Init(desc.window, desc.swapChainBufferCount, desc.fullscreen, desc.swapChainFormat, desc.stencilUsage, desc.depthStencilFormat, desc.depthStencilMode);
				else return swapChainD3D12.Init(desc.window, desc.swapChainBufferCount, desc.fullscreen, desc.swapChainFormat);
			}
			else
			{
				return true;
			}
		}

		public override void Dispose()
		{
			swapChain = null;
			if (swapChainD3D12 != null)
			{
				swapChainD3D12.Dispose();
				swapChainD3D12 = null;
			}

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			Orbital_Video_D3D12_Device_BeginFrame(handle);
			if (type == DeviceType.Presentation) swapChainD3D12.BeginFrame();
		}

		public override void EndFrame()
		{
			if (type == DeviceType.Presentation) swapChainD3D12.Present();
			Orbital_Video_D3D12_Device_EndFrame(handle);
		}

		public unsafe override bool GetMaxMSAALevel(TextureFormat format, out MSAALevel msaaLevel)
		{
			var result = MSAALevel.Disabled;
			if (Orbital_Video_D3D12_Device_GetMaxMSAALevel(handle, format, &result) != 0)
			{
				msaaLevel = result;
				return true;
			}
			msaaLevel = MSAALevel.Disabled;
			return false;
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format)
		{
			var abstraction = new SwapChain(this, ensureSizeMatchesWindowSize);
			if (!abstraction.Init(window, bufferCount, fullscreen, format))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
		}

		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode)
		{
			var abstraction = new SwapChain(this, ensureSizeMatchesWindowSize);
			if (!abstraction.Init(window, bufferCount, fullscreen, format, stencilUsage, depthStencilFormat, depthStencilMode))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
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
			return swapChainD3D12.CreateRenderPass(desc);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil)
		{
			return swapChainD3D12.CreateRenderPass(desc, depthStencil);
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures)
		{
			var abstraction = new RenderPass(this);
			if (!abstraction.Init(desc, (RenderTexture2D[])renderTextures))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderState");
			}
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures, DepthStencilBase depthStencil)
		{
			var abstraction = new RenderPass(this);
			if (!abstraction.Init(desc, (RenderTexture2D[])renderTextures, (DepthStencil)depthStencil))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderState");
			}
			return abstraction;
		}

		public override RenderStateBase CreateRenderState(RenderStateDesc desc)
		{
			var abstraction = new RenderState(this);
			if (!abstraction.Init(desc))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderState");
			}
			return abstraction;
		}

		public override ShaderEffectBase CreateShaderEffect(Stream stream, ShaderSamplerAnisotropy anisotropyOverride)
		{
			var abstraction = new ShaderEffect(this);
			if (!abstraction.Init(stream, anisotropyOverride))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ShaderEffect");
			}
			return abstraction;
		}

		public override ShaderEffectBase CreateShaderEffect(ShaderBase vs, ShaderBase ps, ShaderBase hs, ShaderBase ds, ShaderBase gs, ShaderEffectDesc desc, bool disposeShaders)
		{
			var abstraction = new ShaderEffect(this);
			if (!abstraction.Init((Shader)vs, (Shader)ps, (Shader)hs, (Shader)ds, (Shader)gs, desc, disposeShaders))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ShaderEffect");
			}
			return abstraction;
		}

		public override ComputeStateBase CreateComputeState(ComputeStateDesc desc)
		{
			var abstraction = new ComputeState(this);
			if (!abstraction.Init(desc))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ComputeState");
			}
			return abstraction;
		}

		public override ComputeShaderBase CreateComputeShader(Stream stream, ComputeShaderDesc desc)
		{
			var abstraction = new ComputeShader(this);
			if (!abstraction.Init(stream, desc))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ComputeShader");
			}
			return abstraction;
		}

		public override ComputeShaderBase CreateComputeShader(byte[] bytecode, ComputeShaderDesc desc)
		{
			var abstraction = new ComputeShader(this);
			if (!abstraction.Init(bytecode, desc))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ComputeShader");
			}
			return abstraction;
		}

		public override ComputeShaderBase CreateComputeShader(byte[] bytecode, int offset, int length, ComputeShaderDesc desc)
		{
			var abstraction = new ComputeShader(this);
			if (!abstraction.Init(bytecode, offset, length, desc))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ComputeShader");
			}
			return abstraction;
		}

		public override VertexBufferBase CreateVertexBuffer(uint vertexCount, uint vertexSize, VertexBufferMode mode)
		{
			var abstraction = new VertexBuffer(this, mode);
			if (!abstraction.Init(vertexCount, vertexSize))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create VertexBuffer");
			}
			return abstraction;
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, VertexBufferMode mode)
		{
			var abstraction = new VertexBuffer(this, mode);
			if (!abstraction.Init<T>(vertices))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create VertexBuffer");
			}
			return abstraction;
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, ushort[] indices, VertexBufferMode mode)
		{
			var abstraction = new VertexBuffer(this, mode);
			if (!abstraction.Init<T>(vertices, indices))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create VertexBuffer");
			}
			return abstraction;
		}

		public override VertexBufferBase CreateVertexBuffer<T>(T[] vertices, uint[] indices, VertexBufferMode mode)
		{
			var abstraction = new VertexBuffer(this, mode);
			if (!abstraction.Init<T>(vertices, indices))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create VertexBuffer");
			}
			return abstraction;
		}

		public override IndexBufferBase CreateIndexBuffer(uint indexCount, IndexBufferSize indexSize, IndexBufferMode mode)
		{
			var abstraction = new IndexBuffer(this, mode);
			if (!abstraction.Init(indexCount, indexSize))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create IndexBuffer");
			}
			return abstraction;
		}

		public override IndexBufferBase CreateIndexBuffer(ushort[] indices, IndexBufferMode mode)
		{
			var abstraction = new IndexBuffer(this, mode);
			if (!abstraction.Init(indices))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create IndexBuffer");
			}
			return abstraction;
		}

		public override IndexBufferBase CreateIndexBuffer(uint[] indices, IndexBufferMode mode)
		{
			var abstraction = new IndexBuffer(this, mode);
			if (!abstraction.Init(indices))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create IndexBuffer");
			}
			return abstraction;
		}

		public override VertexBufferStreamerBase CreateVertexBufferStreamer(VertexBufferStreamLayout layout)
		{
			var abstraction = new VertexBufferStreamer(this);
			if (!abstraction.Init(layout))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create IndexBuffer");
			}
			return abstraction;
		}

		public override ConstantBufferBase CreateConstantBuffer(int size, ConstantBufferMode mode)
		{
			var abstraction = new ConstantBuffer(this, mode);
			if (!abstraction.Init(size))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ConstantBuffer");
			}
			return abstraction;
		}

		public override ConstantBufferBase CreateConstantBuffer<T>(ConstantBufferMode mode)
		{
			var abstraction = new ConstantBuffer(this, mode);
			if (!abstraction.Init<T>())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ConstantBuffer");
			}
			return abstraction;
		}

		public override ConstantBufferBase CreateConstantBuffer<T>(T initialData, ConstantBufferMode mode)
		{
			var abstraction = new ConstantBuffer(this, mode);
			if (!abstraction.Init<T>(initialData))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create ConstantBuffer");
			}
			return abstraction;
		}

		public override Texture2DBase CreateTexture2D(int width, int height, TextureFormat format, byte[] data, TextureMode mode)
		{
			var abstraction = new Texture2D(this, mode);
			if (!abstraction.Init(width, height, format, data))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create Texture2D");
			}
			return abstraction;
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, MSAALevel msaaLevel, bool allowRandomAccess)
		{
			var abstraction = new RenderTexture2D(this, usage, mode);
			if (!abstraction.Init(width, height, format, msaaLevel, allowRandomAccess))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderTexture2D");
			}
			return abstraction;
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode)
		{
			var abstraction = new RenderTexture2D(this, usage, mode);
			if (!abstraction.Init(width, height, format, data))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderTexture2D");
			}
			return abstraction;
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, MSAALevel msaaLevel, bool allowRandomAccess)
		{
			var abstraction = new RenderTexture2D(this, usage, mode);
			if (!abstraction.Init(width, height, format, stencilUsage, depthStencilFormat, depthStencilMode, msaaLevel, allowRandomAccess))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderTexture2D");
			}
			return abstraction;
		}

		public override Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, bool allowRandomAccess)
		{
			var abstraction = new RenderTexture2D(this, usage, mode);
			if (!abstraction.Init(width, height, format, data, stencilUsage, depthStencilFormat, depthStencilMode, allowRandomAccess))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderTexture2D");
			}
			return abstraction;
		}

		public override DepthStencilBase CreateDepthStencil(int width, int height, DepthStencilFormat format, StencilUsage stencilUsage, DepthStencilMode mode, MSAALevel msaaLevel)
		{
			var abstraction = new DepthStencil(this, stencilUsage, mode);
			if (!abstraction.Init(width, height, format, msaaLevel))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create DepthStencil");
			}
			return abstraction;
		}
		#endregion
	}
}
