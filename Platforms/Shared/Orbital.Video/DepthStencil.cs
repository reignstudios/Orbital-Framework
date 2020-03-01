using System;

namespace Orbital.Video
{
	public enum DepthStencilMode
	{
		/// <summary>
		/// Memory will be optimized for GPU only use
		/// </summary>
		GPUOptimized
	}

	public enum DepthStencilFormat
	{
		/// <summary>
		/// Let the API choose depth only format
		/// </summary>
		DefaultDepth,

		/// <summary>
		/// Let the API choose depth + stencil format
		/// </summary>
		DefaultDepthStencil,

		/// <summary>
		/// 32bit depth
		/// </summary>
		D32,

		/// <summary>
		/// 32bit depth + 8bit stencil
		/// </summary>
		D32S8,

		/// <summary>
		/// 24bit depth + 8bit stencil
		/// </summary>
		D24S8,

		/// <summary>
		/// 16bit depth
		/// </summary>
		D16
	}

	public enum StencilUsage
	{
		/// <summary>
		/// Discards previous stencil data.
		/// Can be more optimized if stencil is not used.
		/// </summary>
		Discard,

		/// <summary>
		/// Preserves previous stencil data.
		/// </summary>
		Preserve
	}

	public abstract class DepthStencilBase : IDisposable
	{
		public readonly DeviceBase device;
		public int width { get; protected set; }
		public int height { get; protected set; }
		public readonly StencilUsage stencilUsage;

		public DepthStencilBase(DeviceBase device, StencilUsage usage)
		{
			this.device = device;
			this.stencilUsage = usage;
		}

		public abstract void Dispose();

		/// <summary>
		/// Returns pointer to platform specific native handle
		/// </summary>
		public abstract IntPtr GetHandle();
	}
}
