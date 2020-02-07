using System;

namespace Orbital.Video
{
	public enum VertexBufferMode
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

	public enum VertexBufferTopology
	{
		Point,
		Line,
		Triangle
	}

	public abstract class VertexBufferBase : IDisposable
	{
		public readonly DeviceBase device;
		public IndexBufferBase indexBuffer { get; protected set; }
		public int vertexCount { get; protected set; }
		public int vertexSize { get; protected set; }

		public VertexBufferBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		protected IndexBufferMode GetIndexBufferMode(VertexBufferMode mode)
		{
			switch (mode)
			{
				case VertexBufferMode.GPUOptimized: return IndexBufferMode.GPUOptimized;
				case VertexBufferMode.Write: return IndexBufferMode.Write;
				case VertexBufferMode.Read: return IndexBufferMode.Read;
			}
			throw new NotSupportedException("VertexBufferMode not compatible with IndexBufferMode");
		}
	}
}
