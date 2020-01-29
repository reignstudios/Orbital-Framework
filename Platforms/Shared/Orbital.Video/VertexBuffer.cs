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

	public enum VertexBufferLayoutElementType
	{
		Float,
		Float2,
		Float3,
		Float4,
		RGBAx8
	}

	public enum VertexBufferLayoutElementUsage
	{
		Position,
		Color,
		UV,
		Normal,
		Tangent,
		Binormal,
		Index,
		Weight
	}

	public enum VertexBufferLayoutStreamType
	{
		/// <summary>
		/// Common vertex buffer data
		/// </summary>
		VertexData,

		/// <summary>
		/// Data that is specific to the instance stream and increments per mesh instead of per vertex
		/// </summary>
		InstanceData
	}

	public struct VertexBufferLayoutElement
	{
		/// <summary>
		/// The element data type
		/// </summary>
		public VertexBufferLayoutElementType type;

		/// <summary>
		/// How the element will be used
		/// </summary>
		public VertexBufferLayoutElementUsage usage;

		/// <summary>
		/// Which slot will the usage be used
		/// </summary>
		public int usageIndex;

		/// <summary>
		/// How the data is streamed
		/// </summary>
		public VertexBufferLayoutStreamType streamType;

		/// <summary>
		/// The vertex buffer stream index
		/// </summary>
		public int streamIndex;

		/// <summary>
		/// The byte offset in the vertex buffer stream
		/// </summary>
		public int streamByteOffset;
	}

	public struct VertexBufferLayout
	{
		public VertexBufferLayoutElement[] elements;
	}

	public abstract class VertexBufferBase : IDisposable
	{
		public int vertexCount { get; protected set; }
		public int vertexSize { get; protected set; }

		public abstract void Dispose();
	}
}
