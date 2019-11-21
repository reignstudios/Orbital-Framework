using System;
using Orbital.Host;
using Orbital.Host.WinForms;

using Forms = System.Windows.Forms;

namespace Orbital.Demo.WinForms
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			// init app and window
			var application = new Application();
			var window = new Window(0, 0, 320, 240, WindowSizeType.WorkingArea, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: WinForms");
			window.Show();

			// run example
			using (var example = new Example(application, window))
			{
				#if NET_CORE
				example.Init(@"..\..\..\..\..", "x64", "Win32");
				#elif NET_FRAMEWORK
				example.Init(@"..\..\..\..", "x64", "Win32");
				#endif
				example.Run();
			}
		}
	}
}
