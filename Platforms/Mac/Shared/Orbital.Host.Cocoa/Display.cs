using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Orbital.Host.Cocoa
{
	public unsafe static class Displays
	{
		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Display_GetMainScreen(int* width, int* height);
		
		[DllImport(Native.lib)]
		private static extern int Orbital_Host_Display_GetAllScreensCount();

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Display_GetAllScreens(int** widths, int** heights, int count);
		
		public static Display GetPrimaryDisplay()
		{
			int width, height;
			Orbital_Host_Display_GetMainScreen(&width, &height);
			return new Display(true, width, height);
		}

		public static Display[] GetDisplays()
		{
			int count = Orbital_Host_Display_GetAllScreensCount();
			int* widths = stackalloc int[count];
			int* heights = stackalloc int[count];
			Orbital_Host_Display_GetAllScreens(&widths, &heights, count);
			var displays = new Display[count];
			for (int i = 0; i < count; ++i)
			{
				displays[i] = new Display(i == 0, widths[i], heights[i]);
			}
			return displays;
		}
	}
}
