using System;
using System.Collections.Generic;

namespace Orbital.Host
{
	public struct Display
	{
		public bool isPrimary;
		public int width, height;

		public Display(bool isPrimary, int width, int height)
		{
			this.isPrimary = isPrimary;
			this.width = width;
			this.height = height;
		}
	}
}
