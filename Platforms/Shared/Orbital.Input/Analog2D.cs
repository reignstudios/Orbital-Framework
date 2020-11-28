using Orbital.Numerics;

namespace Orbital.Input
{
	public struct Analog2D
	{
		public string name;
		public Vec2 value;
		public float tolerance;

		public void Update(Vec2 value)
		{
			if (value.Length() <= tolerance) value = Vec2.zero;
			this.value = value;
		}
	}
}
