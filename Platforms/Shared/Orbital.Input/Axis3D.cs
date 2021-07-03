using Orbital.Numerics;

namespace Orbital.Input
{
	public class Axis3D
	{
		/// <summary>
		/// Any input under talerance will be forced to Vec3.zero
		/// </summary>
		public float tolerance = .1f;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .75f;

		/// <summary>
		/// Value of the axis input
		/// </summary>
		public Vec3 value { get; private set; }

		public void Update(Vec3 value)
		{
			if (value.Length() <= tolerance) value = Vec3.zero;
			this.value += (value - this.value) * smoothing;
		}
	}

	struct Axis3DNameMap
	{
		public Axis3D axis;
		public string name;

		public Axis3DNameMap(Axis3D axis, string name)
		{
			this.axis = axis;
			this.name = name;
		}
	}
}
