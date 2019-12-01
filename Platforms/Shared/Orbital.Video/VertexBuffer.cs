namespace Orbital.Video
{
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

	public struct VertexBufferLayoutElement
	{
		public VertexBufferLayoutElementType type;
		public VertexBufferLayoutElementUsage usage;
		public int streamIndex, usageIndex, byteOffset;
	}

	public struct VertexBufferLayout
	{
		public VertexBufferLayoutElement[] elements;
	}
}
