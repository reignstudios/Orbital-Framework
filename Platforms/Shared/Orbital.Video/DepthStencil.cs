using System;

namespace Orbital.Video
{
	public enum DepthStencilFormat
	{
		Default,
		D24S8
	}

	public abstract class DepthStencilBase : IDisposable
	{
		public readonly DeviceBase device;

		public DepthStencilBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();
	}
}
