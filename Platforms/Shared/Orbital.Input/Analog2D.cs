using Orbital.Numerics;

namespace Orbital.Input
{
	public struct Analog2D
	{
		public Vec2 value { get; private set; }

		internal void Update(Vec2 value, float tolerance)
		{
			if (value.Length() <= tolerance) value = Vec2.zero;
			this.value = value;
		}
	}
}
