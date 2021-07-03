namespace Orbital.Input
{
	public class Slider
	{
		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .25f;

		/// <summary>
		/// Value of the slider input
		/// </summary>
		public float value { get; private set; }

		public void Update(float value)
		{
			if (smoothing < 0.0f) smoothing = 0.0f;
			if (smoothing > 1.0f) smoothing = 1.0f;
			this.value += (value - this.value) * smoothing;
		}
	}

	struct SliderNameMap
	{
		public Slider slider;
		public string name;

		public SliderNameMap(Slider slider, string name)
		{
			this.slider = slider;
			this.name = name;
		}
	}
}
