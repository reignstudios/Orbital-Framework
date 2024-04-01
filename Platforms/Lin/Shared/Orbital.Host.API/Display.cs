using System.Runtime.InteropServices;

namespace Orbital.Host.API
{
	public static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			switch (Application.api)
			{
				case ApplicationAPI.X11: return X11.Displays.GetPrimaryDisplay();
				case ApplicationAPI.Wayland: return Wayland.Displays.GetPrimaryDisplay();
				case ApplicationAPI.GTK3: return GTK3.Displays.GetPrimaryDisplay();
				case ApplicationAPI.GTK4: return GTK4.Displays.GetPrimaryDisplay();
				default: throw new NotImplementedException();
			}
		}

		public static Display[] GetDisplays()
		{
			switch (Application.api)
			{
				case ApplicationAPI.X11: return X11.Displays.GetDisplays();
				case ApplicationAPI.Wayland: return Wayland.Displays.GetDisplays();
				case ApplicationAPI.GTK3: return GTK3.Displays.GetDisplays();
				case ApplicationAPI.GTK4: return GTK4.Displays.GetDisplays();
				default: throw new NotImplementedException();
			}
		}
	}
}
