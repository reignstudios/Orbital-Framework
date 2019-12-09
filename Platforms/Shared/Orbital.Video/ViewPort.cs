using Orbital.Numerics;

namespace Orbital.Video
{
	public struct ViewPort
	{
		public Rect2 rect;
		public float minDepth, maxDepth;

		public ViewPort(Rect2 rect)
		{
			this.rect = rect;
			minDepth = 0;
			maxDepth = 1;
		}

		public ViewPort(Rect2 rect, float minDepth, float maxDepth)
		{
			this.rect = rect;
			this.minDepth = minDepth;
			this.maxDepth = maxDepth;
		}

		public float GetAspect()
		{
			return rect.size.width / rect.size.height;
		}
	}
}
