﻿using Orbital.Numerics;

namespace Orbital.Input
{
	public class Analog2D
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

		public Analog2D(bool attached)
		{
			this.attached = attached;
		}

		public void Update(Vec2 value)
		{
			if (value.Length() <= tolerance) value = Vec2.zero;
			this.value += (value - this.value) * smoothing;
		}
	}
}
