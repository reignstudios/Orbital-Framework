using System;

namespace Orbital.Video
{
	public enum SwapChainFormat
	{
		/// <summary>
		/// Let the API choose default non-HDR format
		/// </summary>
		Default,

		/// <summary>
		/// Let the API choose default HDR format
		/// </summary>
		DefaultHDR,

		/// <summary>
		/// 32bit: 8bit for RGBA non-floating point channels
		/// </summary>
		B8G8R8A8,

		/// <summary>
		/// 32bit: 10bit for RGB floating point channels + 2bit for alpha as non-floating point channel
		/// </summary>
		R10G10B10A2
	}

	public abstract class SwapChainBase : IDisposable
	{
		public readonly DeviceBase device;
		public DepthStencilBase depthStencil { get; protected set; }

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
