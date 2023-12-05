using System;
using System.Collections.Generic;
using AppKit;

namespace Orbital.Host.Mac
{
	public static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			var size = NSScreen.MainScreen.Frame.Size;
			return new Display(true, (int)size.Width, (int)size.Height);
		}

		public static Display[] GetDisplays()
		{
			var screens = NSScreen.Screens;
			var displays = new Display[screens.Length];
			for (int i = 0; i != displays.Length; ++i)
			{
				var size = screens[i].Frame.Size;
				displays[i] = new Display(i == 0, (int)size.Width, (int)size.Height);
			}
			return displays;
		}
	}
}
