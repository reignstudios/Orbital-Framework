using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Orbital.Host.Carbon
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
