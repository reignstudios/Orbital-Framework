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
		public readonly DeviceBase device;

		public RenderPassBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();
	}
}
