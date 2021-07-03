using System;

namespace Orbital.Input
{
	public enum Axis1DUpdateMode
	{
		/// <summary>
		/// Value can be positive or negitive
		/// </summary>
		Bidirectional,

		/// <summary>
		/// (0)-(+1) values
		/// </summary>
		Positive,

		/// <summary>
		/// (0)-(-1) values
		/// </summary>
		Negitive,

		/// <summary>
		/// (-1)-(+1) shifted into range of (0)-(+1)
		/// </summary>
		FullRange_ShiftedPositive
	}

	public class Axis1D
	{
		/// <summary>
		/// How the update values are processed
		/// </summary>
		public readonly Axis1DUpdateMode updateMode;

		/// <summary>
		/// Any input under talerance will be forced to 0
		/// </summary>
		public float tolerance = .2f;

		/// <summary>
		/// 0-1 smoothing value
		/// </summary>
		public float smoothing = .75f;

		/// <summary>
		/// Value of the axis input
		/// </summary>
		public float value { get; private set; }

		public Axis1D(Axis1DUpdateMode mode)
		{
			this.updateMode = mode;
		}

		public void Update(float value)
		{
			if (updateMode == Axis1DUpdateMode.Bidirectional)
			{
				if (MathF.Abs(value) <= tolerance) value = 0;
			}
			else if (updateMode == Axis1DUpdateMode.Positive)
			{
				if (value < 0) value = 0;
				if (value <= tolerance) value = 0;
			}
			else if (updateMode == Axis1DUpdateMode.Negitive)
			{
				if (value > 0) value = 0;
				value = MathF.Abs(value);
				if (value <= tolerance) value = 0;
			}
			else if (updateMode == Axis1DUpdateMode.FullRange_ShiftedPositive)
			{
				value += 1f;
				value *= .5f;
				if (value <= tolerance) value = 0;
			}

			this.value += (value - this.value) * smoothing;
		}
	}

	struct Axis1DNameMap
	{
		public Axis1D axis;
		public string name;

		public Axis1DNameMap(Axis1D axis, string name)
		{
			this.axis = axis;
			this.name = name;
		}
	}
}
