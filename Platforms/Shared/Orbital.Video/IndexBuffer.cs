using System;

namespace Orbital.Video
{
	public enum IndexBufferMode
	{
		/// <summary>
		/// Memory will be optimized for GPU only use
		/// </summary>
		GPUOptimized,

		/// <summary>
		/// Memory will be frequently written to by CPU
		/// </summary>
		Write,

		/// <summary>
		/// Memory will be frequently read from the CPU
		/// </summary>
		Read
	}

	public enum IndexBufferSize
	{
		Bit_16 = 16,
		Bit_32 = 32
	}

	public abstract class IndexBufferBase : IDisposable
	{
		public readonly DeviceBase device;
		public int indexCount { get; protected set; }
		public IndexBufferSize indexSize { get; protected set; }

		public IndexBufferBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();
	}
}
