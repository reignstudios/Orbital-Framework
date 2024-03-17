namespace Orbital.Host.GTK3
{
	public unsafe static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			var rect = new GTK3.GdkRectangle();
			GTK3.gdk_monitor_get_geometry(GTK3.gdk_display_get_primary_monitor(GTK3.gdk_display_get_default()), &rect);
			return new Display(true, rect.width, rect.height);
		}

		public unsafe static Display[] GetDisplays()
		{
			IntPtr gtkDisplay = GTK3.gdk_display_get_default();
			IntPtr primaryMonitor = GTK3.gdk_display_get_primary_monitor(gtkDisplay);
			int displayCount = GTK3.gdk_display_get_n_monitors(GTK3.gdk_display_get_default());
			var displays = new Display[displayCount];
			for (int i = 0; i < displayCount; ++i)
			{
				var rect = new GTK3.GdkRectangle();
				IntPtr monitor = GTK3.gdk_display_get_monitor(gtkDisplay, i);
				GTK3.gdk_monitor_get_geometry(monitor, &rect);
				displays[i] = new Display(monitor == primaryMonitor, rect.width, rect.height);
			}
			return displays;
		}
	}
}
