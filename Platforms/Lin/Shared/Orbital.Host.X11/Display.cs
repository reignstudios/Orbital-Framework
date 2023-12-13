namespace Orbital.Host.X11
{
	public static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			int sc = X11.XDefaultScreen(Application.dc);
			IntPtr screen = X11.XScreenOfDisplay(Application.dc, sc);
			return new Display()
			{
				isPrimary = true,
				width = X11.XWidthOfScreen(screen),
				height = X11.XHeightOfScreen(screen)
			};
		}

		public unsafe static Display[] GetDisplays()
		{
			int sc = X11.XDefaultScreen(Application.dc);
			int screenCount = X11.XScreenCount(Application.dc);
			var result = new Display[screenCount];
			for (int i = 0; i != screenCount; ++i)
			{
				IntPtr screen = X11.XScreenOfDisplay(Application.dc, i);
				result[i] = new Display(i == sc, X11.XWidthOfScreen(screen), X11.XHeightOfScreen(screen));
			}
			return result;
		}
	}
}
