using System;
using System.IO;
using Orbital.Host;
using Orbital.Host.Win32;

namespace Orbital.Demo.Win32
{
	static class Program
	{
		static void Main(string[] args)
		{
			// init app and window
			var application = new Application();
			var window = new Window(0, 0, 320, 240, WindowSizeType.WorkingArea, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: Win32");
			window.Show();

			// run example
			using (var example = new Example(application, window))
			{
				#if NET_CORE
				example.Init(@"..\..\..\..\..", "x64", "Win32");
				#elif NET_FRAMEWORK
				example.Init(@"..\..\..\..", "x64", "Win32");
				#elif CS2X
				example.Init(@"..\..\..\..", "x64", "Win32");
				#else
				throw new NotImplementedException();
				#endif
				example.Run();
			}
		}
	}
}
