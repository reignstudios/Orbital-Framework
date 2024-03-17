namespace Orbital.Host.GTK4
{
	public unsafe static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			var rect = new GTK4.GdkRectangle();
			IntPtr display = GTK4.gdk_display_get_default();
			IntPtr surface = GTK4.gdk_surface_new_toplevel(display);
			IntPtr monitor = GTK4.gdk_display_get_monitor_at_surface(display, surface);
			GTK4.gdk_monitor_get_geometry(monitor, &rect);
			return new Display(true, rect.width, rect.height);
		}

		public static Display[] GetDisplays()
		{
			IntPtr display = GTK4.gdk_display_get_default();
			IntPtr surface = GTK4.gdk_surface_new_toplevel(display);
			IntPtr primaryMonitor = GTK4.gdk_display_get_monitor_at_surface(display, surface);
			IntPtr monitorList = GTK4.gdk_display_get_monitors(display);
			uint monitorCount = GTK4.g_list_model_get_n_items(monitorList);
			var displays = new Display[monitorCount];
			for (uint i = 0; i != monitorCount; ++i)
			{
				var rect = new GTK4.GdkRectangle();
				IntPtr monitor = GTK4.g_list_model_get_item(monitorList, i);
				GTK4.gdk_monitor_get_geometry(monitor, &rect);
				bool isPrimary = (monitor == IntPtr.Zero || monitorCount == 1) ? (i == 0) : (monitor == primaryMonitor);
				displays[i] = new Display(isPrimary, rect.width, rect.height);
			}
			return displays;
		}
	}
}
