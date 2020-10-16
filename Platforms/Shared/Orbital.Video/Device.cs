using System;
using System.IO;
using Orbital.Host;

namespace Orbital.Video
{
	/// <summary>
	/// How a device will be used
	/// </summary>
	public enum DeviceType
	{
		/// <summary>
		/// Device will be used for presenting rendered buffers on a physical screen
		/// </summary>
		Presentation,

		/// <summary>
		/// Device will only be used for background processing (such as Compute-Shaders, UI-Embedding, etc)
		/// </summary>
		Background
	}

	/// <summary>
	/// The resource visibility/accessability has accross multiple GPU nodes
	/// </summary>
	public enum MultiGPUNodeResourceVisibility
	{
		/// <summary>
		/// Resource can only be accessed on the GPU it was created.
		/// NOTE: This is the default and should be used unless you need to copy resources between GPUs
		/// </summary>
		Self,

		/// <summary>
		/// Resource can be accessed on all GPUs for special copy operations
		/// </summary>
		All
	}

	public abstract class DeviceBase : IDisposable
	{
		public readonly InstanceBase instance;
		public readonly DeviceType type;
		public SwapChainBase swapChain { get; protected set; }
		public int nodeCount { get; protected set; }

		public DeviceBase(InstanceBase instance, DeviceType type)
		{
			this.instance = instance;
			this.type = type;
		}

		public abstract void Dispose();

		/// <summary>
		/// Do any prep work needed before new presentation frame
		/// </summary>
		public abstract void BeginFrame();

		/// <summary>
		/// Finish and present frame to physical screen
		/// </summary>
		public abstract void EndFrame();

		/// <summary>
		/// Gets max msaa level.
		/// </summary>
		public abstract bool GetMaxMSAALevel(TextureFormat format, out MSAALevel msaaLevel);

		#region Create Methods
		public abstract SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, SwapChainVSyncMode vSyncMode);
		public abstract SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSizeMatchesWindowSize, SwapChainFormat format, SwapChainType type, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, SwapChainVSyncMode vSyncMode);
		public abstract RasterizeCommandListBase CreateRasterizeCommandList();
		public abstract ComputeCommandListBase CreateComputeCommandList();
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc);
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil);
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures);
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc, Texture2DBase[] renderTextures, DepthStencilBase depthStencil);
		public abstract RenderStateBase CreateRenderState(RenderStateDesc desc);
		public abstract ShaderEffectBase CreateShaderEffect(Stream stream, ShaderSamplerAnisotropy anisotropyOverride);
		public abstract ShaderEffectBase CreateShaderEffect(ShaderBase vs, ShaderBase ps, ShaderBase hs, ShaderBase ds, ShaderBase gs, ShaderEffectDesc desc, bool disposeShaders);
		public abstract ComputeStateBase CreateComputeState(ComputeStateDesc desc);
		public abstract ComputeShaderBase CreateComputeShader(Stream stream, ComputeShaderDesc desc);
		public abstract ComputeShaderBase CreateComputeShader(byte[] bytecode, ComputeShaderDesc desc);
		public abstract ComputeShaderBase CreateComputeShader(byte[] bytecode, int offset, int length, ComputeShaderDesc desc);
		#if CS_7_3
		public abstract VertexBufferBase CreateVertexBuffer<T>(T[] vertices, VertexBufferMode mode) where T : unmanaged;
		public abstract ConstantBufferBase CreateConstantBuffer<T>(T initialData, ConstantBufferMode mode) where T : unmanaged;
		#else
		public abstract VertexBufferBase CreateVertexBuffer<T>(T[] vertices, VertexBufferMode mode) where T : struct;
		public abstract ConstantBufferBase CreateConstantBuffer<T>(T initialData, ConstantBufferMode mode) where T : struct;
		#endif
		public abstract VertexBufferBase CreateVertexBuffer(uint vertexCount, uint vertexSize, VertexBufferMode mode);

		public abstract VertexBufferBase CreateVertexBuffer<T>(T[] vertices, ushort[] indices, VertexBufferMode mode)
		#if CS_7_3
		where T : unmanaged;
		#else
		where T : struct;
		#endif

		public abstract VertexBufferBase CreateVertexBuffer<T>(T[] vertices, uint[] indices, VertexBufferMode mode)
		#if CS_7_3
		where T : unmanaged;
		#else
		where T : struct;
		#endif

		public abstract IndexBufferBase CreateIndexBuffer(uint indexCount, IndexBufferSize indexSize, IndexBufferMode mode);
		public abstract IndexBufferBase CreateIndexBuffer(ushort[] indices, IndexBufferMode mode);
		public abstract IndexBufferBase CreateIndexBuffer(uint[] indices, IndexBufferMode mode);
		public abstract VertexBufferStreamerBase CreateVertexBufferStreamer(VertexBufferStreamLayout layout);
		public abstract ConstantBufferBase CreateConstantBuffer<T>(ConstantBufferMode mode) where T : struct;
		public abstract ConstantBufferBase CreateConstantBuffer(int size, ConstantBufferMode mode);
		public abstract Texture2DBase CreateTexture2D(int width, int height, TextureFormat format, byte[] data, TextureMode mode, MultiGPUNodeResourceVisibility nodeVisibility);
		public abstract Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility);
		public abstract Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, MultiGPUNodeResourceVisibility nodeVisibility);
		public abstract Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility);
		public abstract Texture2DBase CreateRenderTexture2D(int width, int height, TextureFormat format, RenderTextureUsage usage, byte[] data, TextureMode mode, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility);
		public abstract DepthStencilBase CreateDepthStencil(int width, int height, DepthStencilFormat format, StencilUsage stencilUsage, DepthStencilMode mode, MSAALevel msaaLevel);
		#endregion
	}
}
