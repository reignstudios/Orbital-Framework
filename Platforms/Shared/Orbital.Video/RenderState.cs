using System;

namespace Orbital.Video
{
	public enum MSAALevel
	{
		Disabled = 0,
		X2 = 2,
		X4 = 4,
		X8 = 8,
		X16 = 16
	}

	public enum TriangleCulling
	{
		/// <summary>
		/// Double-sided. Both faces will draw
		/// </summary>
		None,

		/// <summary>
		/// Back faces will not draw
		/// </summary>
		Back,

		/// <summary>
		/// Front faces will not draw
		/// </summary>
		Front
	}

	public enum TriangleFillMode
	{
		/// <summary>
		/// Triangles entire surface will draw
		/// </summary>
		Solid,

		/// <summary>
		/// Triangle edges will draw as lines
		/// </summary>
		Wireframe
	}

	public struct RenderStateDesc
	{
		/// <summary>
		/// Render pass this state will be used in
		/// </summary>
		public RenderPassBase renderPass;

		/// <summary>
		/// Shader effect to render geometry with
		/// </summary>
		public ShaderEffectBase shaderEffect;

		/// <summary>
		/// Constant buffers to be accessed in shader effect
		/// </summary>
		public ConstantBufferBase[] constantBuffers;

		/// <summary>
		/// Textures to be accessed in shader effect
		/// </summary>
		public TextureBase[] textures;

		/// <summary>
		/// How the geometry will appear
		/// </summary>
		public VertexBufferTopology vertexBufferTopology;

		/// <summary>
		/// Vertex buffers to use and stream in parallel
		/// </summary>
		public VertexBufferStreamerBase vertexBufferStreamer;

		/// <summary>
		/// Index buffer to use.
		/// If null, the IndexBuffer from the first element of the VertexBufferStreamer will be used
		/// </summary>
		public IndexBufferBase indexBuffer;

		/// <summary>
		/// Enables depth read/write
		/// </summary>
		public bool depthEnable;
		
		/// <summary>
		/// Enables stencil read/write
		/// </summary>
		public bool stencilEnable;

		/// <summary>
		/// Vertex buffers face culling method to use
		/// </summary>
		public TriangleCulling triangleCulling;

		/// <summary>
		/// Vertex buffers fill mode to use
		/// </summary>
		public TriangleFillMode triangleFillMode;

		/// <summary>
		/// Multisample anti-aliasing level
		/// </summary>
		public MSAALevel msaaLevel;
	}

	public abstract class RenderStateBase : IDisposable
	{
		public readonly DeviceBase device;

		public RenderStateBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		protected void InitBase(ref RenderStateDesc desc)
		{
			int constantBufferCount = desc.constantBuffers != null ? desc.constantBuffers.Length : 0;
			if (desc.shaderEffect.constantBufferCount != constantBufferCount) throw new ArgumentException("RenderState constant-buffer count doesn't match ShaderEffect requirements");

			int textureCount = desc.textures != null ? desc.textures.Length : 0;
			if (desc.shaderEffect.textureCount != textureCount) throw new ArgumentException("RenderState texture count doesn't match ShaderEffect requirements");

			if (desc.indexBuffer == null)
			{
				var vertexBuffer = desc.vertexBufferStreamer.vertexBuffers[0];
				desc.indexBuffer = vertexBuffer.indexBuffer;
			}
		}
	}
}
