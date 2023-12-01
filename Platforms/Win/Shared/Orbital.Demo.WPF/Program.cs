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
			var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: WPF");
			window.Show();

			// TODO: run rendering example

			Application.Run(window);
		}
	}
}
