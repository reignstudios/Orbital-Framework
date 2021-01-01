using Orbital.Numerics;

namespace Orbital.Input
{
	public class Analog3D
	{
		/// <summary>
		/// Name of analog
		/// </summary>
		public string name;

		/// <summary>
		/// Is the analog attached to a device
		/// </summary>
		public readonly bool attached;

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

		public Analog3D(bool attached)
		{
			this.attached = attached;
		}

		public void Update(Vec3 value)
		{
			if (value.Length() <= tolerance) value = Vec3.zero;
			this.value += (value - this.value) * smoothing;
		}
	}
}
