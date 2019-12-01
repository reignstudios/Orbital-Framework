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
		public abstract void Dispose();
	}
}
