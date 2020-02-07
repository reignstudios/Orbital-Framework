using System;

namespace Orbital.Video
{
	public abstract class RenderTextureBase : IDisposable
	{
		public readonly DeviceBase device;

		public RenderTextureBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();
	}
}
