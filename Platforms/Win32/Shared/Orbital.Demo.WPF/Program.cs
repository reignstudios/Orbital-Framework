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
			var application = new Application();
			var window = new Window(0, 0, 320, 240, WindowSizeType.WorkingArea, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: WPF");
			window.Show();
			application.Run(window);
		}
	}
}
