namespace Orbital.Input
{
	public struct Analog1D
	{
		public string name;
		public float value;
		public float tolerance;

		public void Update(float value)
		{
			if (value <= tolerance) value = 0;
			this.value = value;
		}
	}
}
