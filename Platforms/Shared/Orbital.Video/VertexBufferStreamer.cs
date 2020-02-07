using System;

namespace Orbital.Video
{
	public enum VertexBufferStreamType
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

	public struct VertexBufferStreamDesc
	{
		/// <summary>
		/// Vertex buffer to stream
		/// </summary>
		public VertexBufferBase vertexBuffer;

		/// <summary>
		/// How the data is streamed
		/// </summary>
		public VertexBufferStreamType type;
	}

	public enum VertexBufferStreamElementType
	{
		Float,
		Float2,
		Float3,
		Float4,
		RGBAx8
	}

	public enum VertexBufferStreamElementUsage
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

	public struct VertexBufferStreamElement
	{
		/// <summary>
		/// Which vertex buffer to stream from
		/// </summary>
		public int index;

		/// <summary>
		/// The element data type
		/// </summary>
		public VertexBufferStreamElementType type;

		/// <summary>
		/// How the element will be used
		/// </summary>
		public VertexBufferStreamElementUsage usage;

		/// <summary>
		/// Which slot will the usage be used
		/// </summary>
		public int usageIndex;

		/// <summary>
		/// The byte offset in the vertex buffer stream
		/// </summary>
		public int offset;
	}

	public struct VertexBufferStreamLayout
	{
		/// <summary>
		/// Vertex buffer descriptions
		/// </summary>
		public VertexBufferStreamDesc[] descs;

		/// <summary>
		/// Describes how vertex buffers will be streamed in parallel with one another
		/// </summary>
		public VertexBufferStreamElement[] elements;

		/// <summary>
		/// Vertex count of primary vertex buffer.
		/// If '0' first vertex buffer count will be used.
		/// </summary>
		public int vertexCount;
	}

	public abstract class VertexBufferStreamerBase : IDisposable
	{
		public int vertexCount { get; protected set; }

		public abstract void Dispose();
	}
}
