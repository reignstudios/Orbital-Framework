﻿using System;
using System.Collections.Generic;

namespace Orbital.Host.Mac
{
	public static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			return new Display();
		}

		public static Display[] GetDisplays()
		{
			return null;
		}
	}
}
