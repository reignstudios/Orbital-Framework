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
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			
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
				example.Init(@"..\..\..\..\..", "x64", "Win32");
				#else
				throw new NotImplementedException();
				#endif
				example.Run();
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			if (ex == null) return;
			Console.WriteLine("Orbital ERROR: " + ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.WriteLine();
			Console.WriteLine("HIT ENTER");
			Console.ReadLine();
		}
	}
}
