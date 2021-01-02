using System;

namespace Orbital.Input
{
	public enum Analog1DUpdateMode
	{
		/// <summary>
		/// Value can be positive or negitive
		/// </summary>
		Bidirectional,

		/// <summary>
		/// 0-1 values
		/// </summary>
		Positive,

		/// <summary>
		/// 0-(-1) values
		/// </summary>
		Negitive
	}

	public class Analog1D
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
		/// How the update values are processed
		/// </summary>
		public readonly Analog1DUpdateMode updateMode;

		/// <summary>
		/// Any input under talerance will be forced to 0
		/// </summary>
		public float tolerance = .2f;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .75f;

		/// <summary>
		/// Value of the analog input
		/// </summary>
		public float value { get; private set; }

		public Analog1D(bool physical, Analog1DUpdateMode mode)
		{
			this.physical = physical;
			this.updateMode = mode;
		}

		public void Update(float value)
		{
			if (updateMode == Analog1DUpdateMode.Bidirectional)
			{
				if (MathF.Abs(value) <= tolerance) value = 0;
			}
			else if (updateMode == Analog1DUpdateMode.Positive)
			{
				if (value < 0) value = 0;
				if (value <= tolerance) value = 0;
			}
			else if (updateMode == Analog1DUpdateMode.Negitive)
			{
				if (value > 0) value = 0;
				value = MathF.Abs(value);
				if (value <= tolerance) value = 0;
			}

			this.value += (value - this.value) * smoothing;
		}
	}
}
