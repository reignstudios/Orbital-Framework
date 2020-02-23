using System;

namespace Orbital.Video
{
	public enum RenderTextureMode
	{
		/// <summary>
		/// Memory will be optimized for GPU only use
		/// </summary>
		GPUOptimized
	}

	public abstract class RenderTextureBase : IDisposable
	{
		public readonly DeviceBase device;
		public DepthStencilBase depthStencil { get; protected set; }

		public RenderTextureBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();
	}
}
