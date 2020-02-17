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
		/// True to launch in fullscreen
		/// </summary>
		public bool fullscreen;
	}

	public sealed class Device : DeviceBase
	{
		public readonly Instance instanceD3D12;
		internal IntPtr handle;
		internal SwapChain swapChain;
		private WindowBase window;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create(IntPtr Instance);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, int softwareRasterizer);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_EndFrame(IntPtr handle);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceD3D12 = instance;
			handle = Orbital_Video_D3D12_Device_Create(instance.handle);
		}

		public bool Init(DeviceDesc desc)
		{
			window = desc.window;
			if (Orbital_Video_D3D12_Device_Init(handle, desc.adapterIndex, (desc.softwareRasterizer ? 1 : 0)) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChain = new SwapChain(this, desc.ensureSwapChainMatchesWindowSize);
				return swapChain.Init(desc.window, desc.swapChainBufferCount, desc.fullscreen);
			}
			else
			{
				return true;
			}
		}

		public override void Dispose()
		{
			if (swapChain != null)
			{
				swapChain.Dispose();
				swapChain = null;
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
			if (type == DeviceType.Presentation) swapChain.BeginFrame();
		}

		public override void EndFrame()
		{
			if (type == DeviceType.Presentation) swapChain.Present();
			Orbital_Video_D3D12_Device_EndFrame(handle);
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSwapChainMatchesWindowSize)
		{
			var abstraction = new SwapChain(this, ensureSwapChainMatchesWindowSize);
			if (!abstraction.Init(window, bufferCount, fullscreen))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
		}

		public override CommandListBase CreateCommandList()
		{
			var abstraction = new CommandList(this);
			if (!abstraction.Init())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create CommandList");
			}
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc)
		{
			return swapChain.CreateRenderPass(desc);
		}

		public override RenderStateBase CreateRenderState(RenderStateDesc desc, int gpuIndex)
		{
			var abstraction = new RenderState(this);
			if (!abstraction.Init(desc, gpuIndex))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderState");
			}
			return abstraction;
		}

		public override ShaderEffectBase CreateShaderEffect(Stream stream, ShaderEffectSamplerAnisotropy anisotropyOverride)
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

		public override Texture2DBase CreateTexture2D(TextureFormat format, int width, int height, byte[] data, TextureMode mode)
		{
			var abstraction = new Texture2D(this, mode);
			if (!abstraction.Init(format, width, height, data))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create Texture2D");
			}
			return abstraction;
		}

		public override DepthStencilBase CreateDepthStencil(DepthStencilFormat format, int width, int height, DepthStencilMode mode)
		{
			var abstraction = new DepthStencil(this, mode);
			if (!abstraction.Init(format, width, height))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create DepthStencil");
			}
			return abstraction;
		}
		#endregion
	}
}
