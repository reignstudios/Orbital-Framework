namespace Orbital.Input
{
	public struct Analog1D
	{
		public float value { get; private set; }

		internal void Update(float value, float tolerance)
		{
			if (value <= tolerance) value = 0;
			this.value = value;
		}
	}
}
