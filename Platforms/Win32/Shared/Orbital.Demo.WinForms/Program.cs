using System;
using Orbital.Host;
using Orbital.Host.WinForms;
using Orbital.Video;
using Orbital.Video.D3D12;

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

			// init video
			using (var device = new Device(DeviceType.Presentation))// TODO: put this logic in shared game demo cs file
			{
				device.Init(FeatureLevel.Level_12_0);

				// run main loop
				//application.Run(window);
				while (!window.IsClosed())
				{
					application.RunEvents();
					System.Threading.Thread.Sleep(1000 / 60);
				}
			}
		}
	}
}
