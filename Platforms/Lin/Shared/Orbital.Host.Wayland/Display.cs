using System.Runtime.InteropServices;

namespace Orbital.Host.Wayland
{
	public unsafe static class Displays
	{
		[StructLayout(LayoutKind.Sequential)]
		struct Screen
		{
			public int isPrimary;
			public int width, height;
		}
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Display_GetPrimaryDisplay(IntPtr app, Screen* screen);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Display_GetDisplayCount(IntPtr app, int* screenCount);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Display_GetDisplays(IntPtr app, Screen** screens);
		
		public static Display GetPrimaryDisplay()
		{
			Screen screen;
			Orbital_Host_Wayland_Display_GetPrimaryDisplay(Application.handle, &screen);
			if (screen.isPrimary == 0) throw new Exception("Failed to get primary screen");
			return new Display(screen.isPrimary != 0, screen.width, screen.height);
		}

		public static Display[] GetDisplays()
		{
			int screenCount;
			Orbital_Host_Wayland_Display_GetDisplayCount(Application.handle, &screenCount);

			var screens = stackalloc Screen[screenCount];
			Orbital_Host_Wayland_Display_GetDisplays(Application.handle, &screens);
				
			var displays = new Display[screenCount];
			for (int i = 0; i != screenCount; ++i)
			{
				var screen = screens[i];
				displays[i] = new Display(screen.isPrimary != 0, screen.width, screen.height);
			}
			
			return displays;
		}
	}
}
