using System;
using System.Runtime.InteropServices;

namespace Orbital.Host.Wayland
{
	public unsafe static class Application
	{
		public const string lib = "libOrbital_Host_Wayland_Native.so";

		[DllImport(lib, ExactSpelling = true)]
		private static extern IntPtr Orbital_Host_Wayland_Application_Create();
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Application_Shutdown(IntPtr app);
		
		[DllImport(lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Application_Init(IntPtr app);
		
		public static IntPtr handle { get; private set; }
		
		public static void Init()
		{
			handle = Orbital_Host_Wayland_Application_Create();
			if (handle == IntPtr.Zero) throw new Exception("Failed to create");

			if (Orbital_Host_Wayland_Application_Init(handle) == 0)
			{
				throw new Exception("Failed to init");
			}
		}

		public static void Shutdown()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Wayland_Application_Shutdown(handle);
				handle = IntPtr.Zero;
			}
		}

		public static void Run()
		{
			// TODO
		}

		public static void Run(Window window)
		{
			// TODO
		}

		public static void RunEvents()
		{
			// TODO
		}

		public static void Exit()
		{
			// TODO
		}
	}
}
