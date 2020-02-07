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
		public int vertexCount { get; protected set; }
		public int vertexSize { get; protected set; }

		public abstract void Dispose();
	}
}
