using System;

namespace Orbital.Video
{
	public abstract class RenderTexture2DBase : Texture2DBase
	{
		public DepthStencilBase depthStencil { get; protected set; }

		public RenderTexture2DBase(DeviceBase device)
		: base(device)
		{}
	}
}
