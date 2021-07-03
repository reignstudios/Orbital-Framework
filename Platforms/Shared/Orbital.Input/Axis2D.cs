using Orbital.Numerics;

namespace Orbital.Input
{
	public class Axis2D
	{
		/// <summary>
		/// Any input under talerance will be forced to Vec2.zero
		/// </summary>
		public float tolerance = .2f;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .75f;

		/// <summary>
		/// Value of the axis input
		/// </summary>
		public Vec2 value { get; private set; }

		public void Update(Vec2 value)
		{
			if (value.Length() <= tolerance) value = Vec2.zero;
			this.value += (value - this.value) * smoothing;
		}
	}

	struct Axis2DNameMap
	{
		public Axis2D axis;
		public string name;

		public Axis2DNameMap(Axis2D axis, string name)
		{
			this.axis = axis;
			this.name = name;
		}
	}
}
