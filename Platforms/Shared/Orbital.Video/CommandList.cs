using System;

namespace Orbital.Video
{
	public abstract class CommandListBase : IDisposable
	{
		public readonly DeviceBase device;

		public CommandListBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		/// <summary>
		/// Start so we can record new commands (clears existing commands)
		/// </summary>
		public abstract void Start();

		/// <summary>
		/// Finish so we can execute commands (no new commands can be added)
		/// </summary>
		public abstract void Finish();

		/// <summary>
		/// Enables device render-target used to present next frame
		/// </summary>
		public abstract void EnabledRenderTarget();

		/// <summary>
		/// Enables device render-target + depth-stencil override used to present next frame.
		/// </summary>
		public abstract void EnabledRenderTarget(DepthStencilBase depthStencil);

		/// <summary>
		/// Enables current swap-chain render-target
		/// </summary>
		public abstract void EnabledRenderTarget(SwapChainBase swapChain);

		/// <summary>
		/// Enables current swap-chain render-target + depth-stencil override used to present next frame.
		/// </summary>
		public abstract void EnabledRenderTarget(SwapChainBase swapChain, DepthStencilBase depthStencil);

		/// <summary>
		/// Enables render-target to draw to
		/// </summary>
		public abstract void EnabledRenderTarget(RenderTargetBase renderTarget);

		/// <summary>
		/// Enables render-target + depth-stencil to draw to
		/// </summary>
		public abstract void EnabledRenderTarget(RenderTargetBase renderTarget, DepthStencilBase depthStencil);

		/// <summary>
		/// Prepares device render-target to be used for presenting
		/// </summary>
		public abstract void EnabledPresent();

		/// <summary>
		/// Prepares render-target to be used for presenting
		/// </summary>
		public abstract void EnabledPresent(SwapChainBase swapChain);

		/// <summary>
		/// Clears render target used by device for presenting
		/// </summary>
		public abstract void ClearRenderTarget(float r, float g, float b, float a);

		/// <summary>
		/// Clears current swap-chain render target
		/// </summary>
		public abstract void ClearRenderTarget(SwapChainBase swapChain, float r, float g, float b, float a);

		/// <summary>
		/// Clears render target
		/// </summary>
		public abstract void ClearRenderTarget(RenderTargetBase renderTarget, float r, float g, float b, float a);
	}
}
