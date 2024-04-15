using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Orbital.Host.UWP
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
