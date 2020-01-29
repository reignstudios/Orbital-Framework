using System;

namespace Orbital.Video
{
	public struct RenderStateDesc
	{
		public RenderPassBase renderPass;
		public ShaderEffectBase shaderEffect;
		public ConstantBufferBase[] constantBuffers;
		public TextureBase[] textures;
		public VertexBufferBase vertexBuffer;
		public IndexBufferBase indexBuffer;
		public VertexBufferTopology vertexBufferTopology;
		public bool depthEnable, stencilEnable;
		public int msaaLevel;
	}

	public abstract class RenderStateBase : IDisposable
	{
		public abstract void Dispose();

		protected void ValidateInit(ref RenderStateDesc desc)
		{
			int constantBufferCount = desc.constantBuffers != null ? desc.constantBuffers.Length : 0;
			if (desc.shaderEffect.constantBufferCount != constantBufferCount) throw new ArgumentException("RenderState constant-buffer count doesn't match ShaderEffect requirements");

			int textureCount = desc.textures != null ? desc.textures.Length : 0;
			if (desc.shaderEffect.textureCount != textureCount) throw new ArgumentException("RenderState texture count doesn't match ShaderEffect requirements");
		}
	}
}
