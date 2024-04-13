﻿using Orbital.Host;
using Orbital.Host.Mir;

namespace Orbital.Demo.Mir
{
	static class Program
	{
		private static void Main(string[] args)
		{
			// init app and window
			Application.Init("org.ReignStudios.Orbital");
			var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: Mir");
			window.Show();

			// run app till quit
			Application.Run(window);
			Application.Shutdown();
		}
	}
}