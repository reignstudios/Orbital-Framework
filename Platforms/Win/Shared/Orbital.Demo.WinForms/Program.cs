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
			var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo: WinForms");
			window.Show();

			/*// run example
			using (var example = new Example(window))
			{
				#if NET_CORE
				example.Init(@"..\..\..\..\..", "x64", "Win");
				#elif NET_FRAMEWORK
				example.Init(@"..\..\..\..", "x64", "Win");
				#endif

				Application.RunEvents();
				while (window.IsClosed())
				{
					example.Run();
					Application.RunEvents();
				}
			}*/

			Application.Run(window);
		}
	}
}
