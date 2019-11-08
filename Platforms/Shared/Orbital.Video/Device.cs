using System;
using Orbital.Host;

namespace Orbital.Video
{
	public enum DeviceType
	{
		/// <summary>
		/// Device will be used for presenting rendered buffers on a physical screen
		/// </summary>
		Presentation,

		/// <summary>
		/// Device will only be used for background processing (such as Compute-Shaders, UI-Embedding, etc)
		/// </summary>
		Background
	}

	public abstract class DeviceBase : IDisposable
	{
		public readonly DeviceType type;

		public DeviceBase(DeviceType type)
		{
			this.type = type;
		}

		public abstract void Dispose();

		/// <summary>
		/// Do any prep work needed before new presentation frame
		/// </summary>
		public abstract void BeginFrame();

		/// <summary>
		/// Finish and present frame to physical screen
		/// </summary>
		public abstract void EndFrame();

		/// <summary>
		/// Executes command-buffer operations
		/// </summary>
		public abstract void ExecuteCommandBuffer(CommandBufferBase commandBuffer);

		#region Create Methods
		public abstract SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen);
		public abstract CommandBufferBase CreateCommandBuffer();
		#endregion
	}
}
