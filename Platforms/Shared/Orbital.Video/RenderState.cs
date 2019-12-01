using System;

namespace Orbital.Video
{
	public struct RenderStateDesc
	{
		public ShaderEffectBase shaderEffect;
		public VertexBufferTopology vertexBufferTopology;
		public VertexBufferLayout vertexBufferLayout;
		public TextureFormat[] renderTargetFormats;
		public DepthStencilFormat depthStencilFormat;
		public bool depthEnable, stencilEnable;
		public int msaaLevel;
	}

	public abstract class RenderStateBase : IDisposable
	{
		public abstract void Dispose();
	}
}
