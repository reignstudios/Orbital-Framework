using System;

namespace Orbital.Video
{
	public abstract class SwapChainBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Swaps back-buffer and presents to display
		/// </summary>
		public abstract void Present();
	}
}
