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

		/// <summary>
		/// Size of the buffer with alignment padding
		/// </summary>
		public int size { get; protected set; }

		public ConstantBufferBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		public abstract bool BeginUpdate();
		public abstract void EndUpdate();

		#if CS_7_3
		public abstract void Update<T>(T data) where T : unmanaged;
		public abstract void Update<T>(T data, int offset) where T : unmanaged;
		#else
		public abstract void Update<T>(T data) where T : struct;
		public abstract void Update<T>(T data, int offset) where T : struct;
		#endif

		public unsafe abstract void Update(void* data, int dataSize, int offset);
	}
}
