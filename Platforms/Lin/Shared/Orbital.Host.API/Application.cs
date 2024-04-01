using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Orbital.Host.API
{
	[Flags]
	public enum ApplicationAPI
	{
		X11 = 1,
		Wayland = 2,
		GTK3 = 4,
		GTK4 = 8
	}
	
	public struct ApplicationDesc
	{
		public ApplicationAPI supportedAPIs;
		public string appID;
	}
	
	public static class Application
	{
		public static ApplicationAPI api { get; private set; }
		
		public static void Init(ApplicationDesc desc)
		{
			string session = Environment.GetEnvironmentVariable("DESKTOP_SESSION");
			if (session != null) Console.WriteLine("Orbital.Host.API: Desktop Session type: " + session);
			
			string sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
			bool isWaylandSession = false;
			if (sessionType != null)
			{
				Console.WriteLine("Orbital.Host.API: XDG Session type: " + sessionType);
				isWaylandSession = sessionType.StartsWith("wayland");
			}
			
			// check if session in Gnome3 or higher
			bool isGnome3Plus = false;
			try
			{
				using (var process = new Process())
				{
					process.StartInfo.FileName = "gnome-shell";
					process.StartInfo.Arguments = "--version";
					process.StartInfo.RedirectStandardOutput = true;
					process.Start();
					process.WaitForExit();
					string result = process.StandardOutput.ReadLine();
					Console.WriteLine("Orbital.Host.API: Gnome Shell version: " + result);
					var values = result.Split(' ');
					if (values.Length >= 3 && values[0] == "GNOME" && values[1] == "Shell")
					{
						string version = values[2];
						int majorVersion;
						if (int.TryParse(version[0].ToString(), out majorVersion))
						{
							isGnome3Plus = majorVersion >= 3;
						}
					}
				}
			}
			catch {}
			Console.WriteLine("Orbital.Host.API: Is Gnome3+: " + isGnome3Plus.ToString());

			// choose GTK if gnome is being used
			bool supportsGTK3 = (desc.supportedAPIs & ApplicationAPI.GTK3) != 0;
			bool supportsGTK4 = (desc.supportedAPIs & ApplicationAPI.GTK4) != 0;
			if (isGnome3Plus && isWaylandSession && (supportsGTK3 || supportsGTK4))
			{
				// try gtk4
				if (supportsGTK4)
				{
					try
					{
						GTK4.Application.Init(desc.appID);
						api = ApplicationAPI.GTK4;
						return;
					}
					catch (Exception e)
					{
						Console.WriteLine("Orbital.Host.API: GTK4 not available");
						GTK4.Application.Shutdown();
					}
				}
				
				// try gkt3
				if (supportsGTK3)
				{
					try
					{
						GTK3.Application.Init(desc.appID);
						api = ApplicationAPI.GTK3;
						return;
					}
					catch (Exception e)
					{
						Console.WriteLine("Orbital.Host.API: GTK3 not available");
						GTK3.Application.Shutdown();
					}
				}
			}
			
			// try wayland
			if ((desc.supportedAPIs & ApplicationAPI.Wayland) != 0)
			{
				try
				{
					Wayland.Application.Init(desc.appID);
					api = ApplicationAPI.Wayland;
					return;
				}
				catch (Exception e)
				{
					Console.WriteLine("Orbital.Host.API: Wayland not available");
					Wayland.Application.Shutdown();
				}
			}
			
			// try x11
			if ((desc.supportedAPIs & ApplicationAPI.X11) != 0)
			{
				try
				{
					X11.Application.Init();
					api = ApplicationAPI.X11;
					return;
				}
				catch (Exception e)
				{
					Console.WriteLine("Orbital.Host.API: X11 not available");
					X11.Application.Shutdown();
				}
			}

			throw new Exception("Failed to init any available API");
		}

		public static void Shutdown()
		{
			switch (api)
			{
				case ApplicationAPI.X11:
					X11.Application.Shutdown();
					break;
				
				case ApplicationAPI.Wayland:
					Wayland.Application.Shutdown();
					break;
				
				case ApplicationAPI.GTK3:
					GTK3.Application.Shutdown();
					break;
				
				case ApplicationAPI.GTK4:
					GTK4.Application.Shutdown();
					break;
				
				default: throw new NotSupportedException();
			}
		}

		public static void Run(string[] args)
		{
			switch (api)
			{
				case ApplicationAPI.X11:
					X11.Application.Run();
					break;
				
				case ApplicationAPI.Wayland:
					Wayland.Application.Run();
					break;
				
				case ApplicationAPI.GTK3:
					GTK3.Application.Run(args);
					break;
				
				case ApplicationAPI.GTK4:
					GTK4.Application.Run(args);
					break;
				
				default: throw new NotSupportedException();
			}
		}

		public static void Run(Window window)
		{
			switch (api)
			{
				case ApplicationAPI.X11:
					X11.Application.Run((X11.Window)window.window);
					break;
				
				case ApplicationAPI.Wayland:
					Wayland.Application.Run((Wayland.Window)window.window);
					break;
				
				case ApplicationAPI.GTK3:
					GTK3.Application.Run((GTK3.Window)window.window);
					break;
				
				case ApplicationAPI.GTK4:
					GTK4.Application.Run((GTK4.Window)window.window);
					break;
				
				default: throw new NotSupportedException();
			}
		}

		public static void RunEvents()
		{
			switch (api)
			{
				case ApplicationAPI.X11:
					X11.Application.RunEvents();
					break;
				
				case ApplicationAPI.Wayland:
					Wayland.Application.RunEvents();
					break;
				
				case ApplicationAPI.GTK3:
					GTK3.Application.RunEvents();
					break;
				
				case ApplicationAPI.GTK4:
					GTK4.Application.RunEvents();
					break;
				
				default: throw new NotSupportedException();
			}
		}

		public static void Exit()
		{
			switch (api)
			{
				case ApplicationAPI.X11:
					X11.Application.Exit();
					break;
				
				case ApplicationAPI.Wayland:
					Wayland.Application.Exit();
					break;
				
				case ApplicationAPI.GTK3:
					GTK3.Application.Exit();
					break;
				
				case ApplicationAPI.GTK4:
					GTK4.Application.Exit();
					break;
				
				default: throw new NotSupportedException();
			}
		}
	}
}
