namespace Orbital.Input
{
	public class Slider
	{
		/// <summary>
		/// Name of button
		/// </summary>
		public string name = "?";

		/// <summary>
		/// Is physically attached to a device. Otherwise virtually simulated
		/// </summary>
		public readonly bool physical;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .25f;

		/// <summary>
		/// Value of the analog input
		/// </summary>
		public float value { get; private set; }

		public Slider(bool physical)
		{
			this.physical = physical;
		}

		public void Update(float value)
		{
			if (smoothing < 0.0f) smoothing = 0.0f;
			if (smoothing > 1.0f) smoothing = 1.0f;
			this.value += (value - this.value) * smoothing;
		}
	}
}
