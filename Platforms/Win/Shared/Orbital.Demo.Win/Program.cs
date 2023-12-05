using System;
using System.IO;
using Orbital.Host;
using Orbital.Host.Win;

namespace Orbital.Demo.Win
{
	static class Program
	{
		static void Main(string[] args)
		{
			//AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			
			// init app and window
			Application.Init();
			var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: Win");
			window.Show();

			/*// run example
			using (var example = new Example(window))
			{
				#if NET_CORE
				example.Init(@"..\..\..\..\..", "x64", "Win");
				#elif NET_FRAMEWORK
				example.Init(@"..\..\..\..", "x64", "Win");
				#elif CS2X
				example.Init(@"..\..\..\..\..", "x64", "Win");
				#else
				throw new NotImplementedException();
				#endif

				Application.RunEvents();
				while (!window.IsClosed())
				{
					example.Run();
					Application.RunEvents();
				}
			}*/

			Application.Run(window);
			Application.Shutdown();
		}

		/*private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			if (ex == null) return;
			Console.WriteLine("Orbital ERROR: " + ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.WriteLine();
			Console.WriteLine("HIT ENTER");
			Console.ReadLine();
		}*/
	}
}
