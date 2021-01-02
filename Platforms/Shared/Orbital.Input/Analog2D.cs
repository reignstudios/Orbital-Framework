using Orbital.Numerics;

namespace Orbital.Input
{
	public class Analog2D
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
		/// Any input under talerance will be forced to Vec2.zero
		/// </summary>
		public float tolerance = .2f;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .75f;

		/// <summary>
		/// Value of the analog input
		/// </summary>
		public Vec2 value { get; private set; }

		public Analog2D(bool physical)
		{
			this.physical = physical;
		}

		public void Update(Vec2 value)
		{
			if (value.Length() <= tolerance) value = Vec2.zero;
			this.value += (value - this.value) * smoothing;
		}
	}
}
