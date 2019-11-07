using System;

namespace Orbital.Video
{
	public abstract class SwapChainBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Does prep work before swap-chain can be used
		/// </summary>
		public abstract void BeginFrame();

		/// <summary>
		/// Swaps back-buffer and presents to display
		/// </summary>
		public abstract void Present();
	}
}
