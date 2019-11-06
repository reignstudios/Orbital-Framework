using System;

namespace Orbital.Video
{
	public abstract class CommandBufferBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Enables render-target to draw to
		/// </summary>
		public abstract void EnabledRenderTarget(RenderTargetBase renderTarget);

		/// <summary>
		/// Enables render-target + depth-stencil to draw to
		/// This method may throw an exception if the abstraction API doesn't support it
		/// </summary>
		public abstract void EnabledRenderTarget(RenderTargetBase renderTarget, DepthStencilBase depthStencil);

		/// <summary>
		/// Enables render-target used to present next frame
		/// </summary>
		public abstract void EnabledRenderTarget(DeviceBase device);

		/// <summary>
		/// Enables render-target + depth-stencil override used to present next frame.
		/// This method may throw an exception if the abstraction API doesn't support it
		/// </summary>
		public abstract void EnabledRenderTarget(DeviceBase device, DepthStencilBase depthStencil);
	}
}
