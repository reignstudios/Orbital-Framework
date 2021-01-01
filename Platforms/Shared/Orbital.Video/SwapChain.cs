using Orbital.Numerics;
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
		R8G8B8A8,

		/// <summary>
		/// 32bit: 10bit for RGB floating point channels + 2bit for alpha as non-floating point channel
		/// </summary>
		R10G10B10A2,

		/// <summary>
		/// 64bit: 16bit for RGBA floating point channels
		/// </summary>
		R16G16B16A16,
	}

	public enum SwapChainType
	{
		/// <summary>
		/// Swap-Chain only uses primary GPU regardless of Multi-GPU support
		/// </summary>
		SingleGPU_Standard,

		/// <summary>
		/// Swap-Chain will init back-buffers on each GPU for AFR rendering.
		/// NOTE: If Multi-GPU support isn't avaliable or enabled will default to 'SingleGPU_Standard'
		/// </summary>
		MultiGPU_AFR
	}

	public enum SwapChainVSyncMode
	{
		/// <summary>
		/// VSync is on (no tearing if possible)
		/// </summary>
		VSyncOn,

		/// <summary>
		/// VSync is off (tearing if possible)
		/// </summary>
		VSyncOff
	}

	public abstract class SwapChainBase : IDisposable
	{
		public readonly DeviceBase device;
		public DepthStencilBase depthStencil { get; protected set; }
		public readonly SwapChainType type;
		public int activeRenderTargetIndex { get; protected set; }
		public int activeNodeIndex { get; protected set; }
		public int lastNodeIndex { get; protected set; }

		public SwapChainBase(DeviceBase device, SwapChainType type)
		{
			this.device = device;

			if (type == SwapChainType.MultiGPU_AFR && device.nodeCount == 1) type = SwapChainType.SingleGPU_Standard;
			this.type = type;
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

		/// <summary>
		/// Resolves/Copies MSAA render-texture to non-MSAA swap-chain
		/// </summary>
		public abstract void ResolveMSAA(Texture2DBase sourceRenderTexture);

		/// <summary>
		/// Copies texture to swap-chain of the same size
		/// </summary>
		public abstract void CopyTexture(Texture2DBase sourceTexture);

		/// <summary>
		/// Copies texture to swap-chain region
		/// </summary>
		public abstract void CopyTexture(Texture2DBase sourceTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel);

		#region Create Methods
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc);
		public abstract RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil);
		#endregion
	}
}
