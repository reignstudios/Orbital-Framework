﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Orbital.Host;
using Orbital.Host.Win;
using Orbital.Input;
using Orbital.Input.DirectInput;

namespace Orbital.Demo.Input.Win
{
	class Program
	{
		static void Main(string[] args)
		{
			/*// init app and window
			Application.Init(Marshal.GetHINSTANCE(typeof(Application).Module));
			var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
			window.SetTitle("Demo.Input: Win");
			window.Show();

			// run example
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
			}

			Application.Shutdown();*/
		}
	}
}
