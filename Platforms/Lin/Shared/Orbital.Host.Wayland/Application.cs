using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using Orbital.OS.Lin;

namespace Orbital.Host.Wayland
{
	public unsafe static class Application
	{
		public const string lib = "libOrbital_Host_Wayland_Native.so";

		[DllImport(lib, ExactSpelling = true)]
		private static extern IntPtr Orbital_Host_Wayland_Application_Create();
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Application_Init(IntPtr app);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Application_Shutdown(IntPtr app);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Application_Run(IntPtr app);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Application_RunEvents(IntPtr app);
		
		public static IntPtr handle { get; private set; }
		internal static byte[] appIDData;
		private static bool exit;
		
		public static void Init(string appID)
		{
			appIDData = Encoding.ASCII.GetBytes(appID + "\0");
			LibraryResolver.Init(Assembly.GetExecutingAssembly());
			
			handle = Orbital_Host_Wayland_Application_Create();
			if (handle == IntPtr.Zero) throw new Exception("Failed to create");

			if (Orbital_Host_Wayland_Application_Init(handle) == 0)
			{
				throw new Exception("Failed to init");
			}
		}

		public static void Shutdown()
		{
			// close all windows
			for (int i = Window._windows.Count - 1; i >= 0; --i)
			{
				Window._windows[i].Close();
			}
			
			// shutdown app
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Wayland_Application_Shutdown(handle);
				handle = IntPtr.Zero;
			}
		}

		public static void Run()
		{
			Orbital_Host_Wayland_Application_Run(handle);
		}

		public static void Run(Window window)
		{
			while (!exit && !window.IsClosed())
			{
				if (Orbital_Host_Wayland_Application_RunEvents(handle) < 0) break;
			}
		}

		public static void RunEvents()
		{
			Orbital_Host_Wayland_Application_RunEvents(handle);
		}

		public static void Exit()
		{
			exit = true;
			Shutdown();
		}
	}
}
