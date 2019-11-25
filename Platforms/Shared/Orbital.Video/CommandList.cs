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
		/// Sets GPU render states and enables one or both of render-target and depth-stencil
		/// </summary>
		public abstract void BeginRenderPass(RenderPassBase renderPass);

		/// <summary>
		/// Ends active render pass
		/// </summary>
		public abstract void EndRenderPass(RenderPassBase renderPass);

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

		/// <summary>
		/// Executes command-list operations
		/// </summary>
		public abstract void Execute();
	}
}
