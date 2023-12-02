using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Orbital.Host.Cocoa
{
	public static class Application
	{
		[DllImport(Native.lib)]
		private static extern IntPtr Orbital_Host_Application_Create();
		
		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Application_Dispose(IntPtr application);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Application_Init(IntPtr application);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Application_Run();

		[DllImport(Native.lib)]
		private static extern int Orbital_Host_Application_RunEvent();
		
		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Application_Quit(IntPtr application);

		[DllImport(Native.lib)]
		private static extern int Orbital_Host_Application_IsQuit(IntPtr application);
		
		public static IntPtr handle { get; private set; }

		public static void Init()
		{
			handle = Orbital_Host_Application_Create();
			Orbital_Host_Application_Init(handle);
		}

		public static void Shutdown()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Application_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public static void Run()
		{
			Orbital_Host_Application_Run();
		}

		public static void Run(Window window)
		{
			while (Orbital_Host_Application_IsQuit(handle) == 0 && !window.IsClosed())
			{
				RunEvents();
			}
		}

		public static void RunEvents()
		{
			Orbital_Host_Application_RunEvent();// only run one event at a time or we can create dead-locks
		}

		public static void Exit()
		{
			Orbital_Host_Application_Quit(handle);
		}
	}
}
