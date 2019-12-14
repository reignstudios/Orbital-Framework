using Orbital.Numerics;
using System;

namespace Orbital.Video
{
	public struct RenderPassDesc
	{
		public bool clearColor, clearDepthStencil;
		public Vec4 clearColorValue;
		public float depthValue, stencilValue;
	}

	public abstract class RenderPassBase : IDisposable
	{
		public abstract void Dispose();
	}
}
