using System;
using Orbital.Host;
using Orbital.Host.WinForms;

namespace Orbital.Demo.WinForms
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			var window = new Window(0, 0, 320, 240, WindowSizeType.WorkingArea, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: WinForms");
			window.Show();
			var application = new Application();
			application.Run(window);
		}
	}
}
