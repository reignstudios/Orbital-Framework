using Orbital.Numerics;

namespace Orbital.Video
{
	public struct RenderPassDesc
	{
		public bool clearColor, clearDepthStencil;
		public Vec4 clearColorValue;
		public float depthValue, stencilValue;
	}
}
