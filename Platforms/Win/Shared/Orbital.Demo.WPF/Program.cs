using System;
using Orbital.Host;
using Orbital.Host.WPF;

namespace Orbital.Demo.WPF
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			// init app and window
			Application.Init();
			var window = new Window("Demo: WPF", 320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);

			// TODO: run rendering example

			Application.Run(window);
			Application.Shutdown();
		}
	}
}
