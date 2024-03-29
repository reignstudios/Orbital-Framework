﻿using System;

namespace Orbital.Host
{
	/// <summary>
	/// Application orientation
	/// </summary>
	[Flags]
	public enum ApplicationOrientation
	{
		All = 0,

		/// <summary>
		/// Landscape left
		/// </summary>
		Landscape = 1,

		/// <summary>
		/// Landscape right
		/// </summary>
		LandscapeFlipped = 2,

		/// <summary>
		/// Portrait up
		/// </summary>
		Portrait = 4,

		/// <summary>
		/// Portrait down
		/// </summary>
		PortraitFlipped = 8
	}
}
