namespace Orbital.Host.GTK3
{
	public unsafe static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			var rect = new GTK3.GdkRectangle();
			IntPtr gtkDisplay = GTK3.gdk_display_get_default();
			IntPtr monitor = GTK3.gdk_display_get_primary_monitor(gtkDisplay);
			if (monitor == IntPtr.Zero)
			{
				monitor = GTK3.gdk_display_get_monitor(gtkDisplay, 0);
				if (monitor == IntPtr.Zero) throw new Exception("Failed to get monitor");
			}
			GTK3.gdk_monitor_get_geometry(monitor, &rect);
			return new Display(true, rect.width, rect.height);
		}

		public static Display[] GetDisplays()
		{
			IntPtr gtkDisplay = GTK3.gdk_display_get_default();
			IntPtr primaryMonitor = GTK3.gdk_display_get_primary_monitor(gtkDisplay);
			int displayCount = GTK3.gdk_display_get_n_monitors(gtkDisplay);
			var displays = new Display[displayCount];
			for (int i = 0; i < displayCount; ++i)
			{
				var rect = new GTK3.GdkRectangle();
				IntPtr monitor = GTK3.gdk_display_get_monitor(gtkDisplay, i);
				GTK3.gdk_monitor_get_geometry(monitor, &rect);
				bool isPrimary = (monitor == IntPtr.Zero || displayCount == 1) ? (i == 0) : (monitor == primaryMonitor);
				displays[i] = new Display(isPrimary, rect.width, rect.height);
			}
			return displays;
		}
	}
}
