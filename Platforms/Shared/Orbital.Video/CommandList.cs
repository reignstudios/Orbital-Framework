using System;
using Orbital.Numerics;

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
		public abstract void EndRenderPass();

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
		public abstract void ClearRenderTarget(Texture2DBase renderTexture, float r, float g, float b, float a);

		/// <summary>
		/// Sets view port
		/// </summary>
		public abstract void SetViewPort(ViewPort viewPort);

		/// <summary>
		/// Sets render state
		/// </summary>
		public abstract void SetRenderState(RenderStateBase renderState);

		/// <summary>
		/// Draw actively set vertex buffer
		/// </summary>
		public abstract void Draw();

		/// <summary>
		/// Resolves/Copies MSAA render-texture to non-MSAA texture
		/// </summary>
		public abstract void ResolveMSAA(Texture2DBase sourceRenderTexture, Texture2DBase destinationRenderTexture);

		/// <summary>
		/// Resolves/Copies MSAA render-texture to non-MSAA swap-chain
		/// </summary>
		public abstract void ResolveMSAA(Texture2DBase sourceRenderTexture, SwapChainBase destinationSwapChain);

		/// <summary>
		/// Copies texture to texture of the same size
		/// </summary>
		public abstract void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture);

		/// <summary>
		/// Copies texture to swap-chain of the same size
		/// </summary>
		public abstract void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain);

		/// <summary>
		/// Copies texture to texture region
		/// </summary>
		public abstract void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel, int destinationMipmapLevel);

		/// <summary>
		/// Copies texture to swap-chain region
		/// </summary>
		public abstract void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel);

		/// <summary>
		/// Executes command-list operations
		/// </summary>
		public abstract void Execute();
	}
}
