using Orbital.Numerics;
using System;

namespace Orbital.Video
{
	public struct RenderPassDesc
	{
		public bool clearColor, clearDepthStencil;
		public Color4F clearColorValue;
		public float depthValue, stencilValue;

		public static RenderPassDesc CreateDefault(Color4F clearColorValue)
		{
			return new RenderPassDesc()
			{
				clearColor = true,
				clearColorValue = clearColorValue,
				clearDepthStencil = true,
				depthValue = 1
			};
		}

		public static RenderPassDesc CreateDefault(Color4F clearColorValue, bool clearDepthStencil)
		{
			return new RenderPassDesc()
			{
				clearColor = true,
				clearColorValue = clearColorValue,
				clearDepthStencil = clearDepthStencil,
				depthValue = 1
			};
		}
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
