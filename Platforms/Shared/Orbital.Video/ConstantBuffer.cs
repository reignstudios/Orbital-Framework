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
		public int size { get; protected set; }

		public abstract void Dispose();
	}
}
