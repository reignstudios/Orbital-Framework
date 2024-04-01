using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using Orbital.OS.Lin;

namespace Orbital.Host.Wayland
{
	public enum ApplicationType
	{
		/// <summary>
		/// The content type none means that either the application has no
		/// data about the content type, or that the content doesn't fit
		/// into one of the other categories.
		/// </summary>
		None,
		
		/// <summary>
		/// The content type photo describes content derived from digital
		/// still pictures and may be presented with minimal processing.
		/// </summary>
		Photo,
		
		/// <summary>
		/// The content type video describes a video or animation and may
		/// be presented with more accurate timing to avoid stutter. Where
		/// scaling is needed, scaling methods more appropriate for video
		/// may be used.
		/// </summary>
		Video,
		
		/// <summary>
		/// The content type game describes a running game. Its content
		/// may be presented with reduced latency.
		/// </summary>
		Game
	}
	
	public unsafe static class Application
	{
		public const string lib = "libOrbital_Host_Wayland_Native.so";

		[DllImport(lib, ExactSpelling = true)]
		private static extern IntPtr Orbital_Host_Wayland_Application_Create();
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Application_Init(IntPtr app, ApplicationType type);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Application_Shutdown(IntPtr app);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Application_Run(IntPtr app);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Application_RunEvents(IntPtr app);
		
		public static IntPtr handle { get; private set; }
		internal static byte[] appIDData;
		private static bool exit;
		
		public static void Init(string appID, ApplicationType type)
		{
			appIDData = Encoding.ASCII.GetBytes(appID + "\0");
			LibraryResolver.Init(Assembly.GetExecutingAssembly());
			
			handle = Orbital_Host_Wayland_Application_Create();
			if (handle == IntPtr.Zero) throw new Exception("Failed to create");

			if (Orbital_Host_Wayland_Application_Init(handle, type) == 0)
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
