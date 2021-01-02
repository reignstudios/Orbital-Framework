using Orbital.Numerics;

namespace Orbital.Input
{
	public class Analog3D
	{
		/// <summary>
		/// Name of analog
		/// </summary>
		public string name = "?";

		/// <summary>
		/// Is physically attached to a device. Otherwise virtually simulated
		/// </summary>
		public readonly bool physical;

		/// <summary>
		/// Any input under talerance will be forced to Vec3.zero
		/// </summary>
		public float tolerance = .1f;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .75f;

		/// <summary>
		/// Value of the analog input
		/// </summary>
		public Vec3 value { get; private set; }

		public Analog3D(bool physical)
		{
			this.physical = physical;
		}

		public void Update(Vec3 value)
		{
			if (value.Length() <= tolerance) value = Vec3.zero;
			this.value += (value - this.value) * smoothing;
		}
	}
}
