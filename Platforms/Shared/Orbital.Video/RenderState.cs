using System;

namespace Orbital.Video
{
	public struct RenderStateDesc
	{
		public RenderPassBase renderPass;
		public ShaderEffectBase shaderEffect;
		public VertexBufferBase vertexBuffer;
		public VertexBufferTopology vertexBufferTopology;
		public bool depthEnable, stencilEnable;
		public int msaaLevel;
	}

	public abstract class RenderStateBase : IDisposable
	{
		public abstract void Dispose();
	}
}
