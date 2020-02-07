using System;

namespace Orbital.Video
{
	public enum ConstantBufferMode
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

	public abstract class ConstantBufferBase : IDisposable
	{
		public readonly DeviceBase device;
		public int size { get; protected set; }

		public ConstantBufferBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		#if CS_7_3
		public abstract bool Update<T>(T data) where T : unmanaged;
		#else
		public abstract bool Update<T>(T data) where T : struct;
		#endif

		public unsafe abstract bool Update(void* data, int dataSize, int dstOffset);
	}
}
