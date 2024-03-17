namespace Orbital.Host.GTK4
{
	public unsafe static class Displays
	{
		public static Display GetPrimaryDisplay()
		{
			return new Display(true, 0, 0);
		}

		public static Display[] GetDisplays()
		{
			var displays = new Display[0];
			return displays;
		}
	}
}
