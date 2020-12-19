namespace Orbital.Input
{
	public class Analog1D
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
		/// Any input under talerance will be forced to 0
		/// </summary>
		public float tolerance = .1f;

		/// <summary>
		/// Value of the analog input
		/// </summary>
		public float value { get; private set; }

		public Analog1D(bool attached)
		{
			this.attached = attached;
		}

		public void Update(float value)
		{
			if (value <= tolerance) value = 0;
			this.value = value;
		}
	}
}
