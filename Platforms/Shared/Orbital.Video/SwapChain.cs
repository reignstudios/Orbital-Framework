using System;

namespace Orbital.Video
{
	public abstract class SwapChainBase : IDisposable
	{
		public readonly DeviceBase device;

		public SwapChainBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		/// <summary>
		/// Does prep work before swap-chain can be used
		/// </summary>
		public abstract void BeginFrame();

		/// <summary>
		/// Swaps back-buffer and presents to display
		/// </summary>
		public abstract void Present();

		#region Create Methods
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc);
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil);
		#endregion
	}
}
