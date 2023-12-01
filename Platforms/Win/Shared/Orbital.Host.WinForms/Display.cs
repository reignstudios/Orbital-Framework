using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Orbital.Host.WinForms
{
	public static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			var screen = Screen.PrimaryScreen;
			var bounds = screen.Bounds;
			return new Display()
			{
				isPrimary = true,
				width = bounds.Width,
				height = bounds.Height
			};
		}

		public static Display[] GetDisplays()
		{
			var screens = Screen.AllScreens;
			var displays = new Display[screens.Length];
			for (int i = 0; i != screens.Length; ++i)
			{
				var screen = screens[i];
				var bounds = screen.Bounds;
				displays[i] = new Display()
				{
					isPrimary = i == 0,
					width = bounds.Width,
					height = bounds.Height
				};
			}
			return displays;
		}
	}
}
