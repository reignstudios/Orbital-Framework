using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Orbital.Host.Cocoa
{
	public sealed class Application : ApplicationBase
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
		private static Thread updateThread;
		private static bool updateThreadRunning;

		public Application()
		{
			handle = Orbital_Host_Application_Create();
			Orbital_Host_Application_Init(handle);

			if (updateThread == null)
			{
				updateThreadRunning = true;
				updateThread = new Thread(Update);
				updateThread.IsBackground = true;
				updateThread.Start();
			}
		}

		public override void Dispose()
		{
			if (updateThread != null)
			{
				updateThreadRunning = false;
				updateThread = null;
			}
			
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Application_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Run()
		{
			Orbital_Host_Application_Run();
		}

		public override void Run(WindowBase window)
		{
			while (Orbital_Host_Application_IsQuit(handle) == 0 && !window.IsClosed())
			{
				RunEvents();
			}
		}

		public override void RunEvents()
		{
			Orbital_Host_Application_RunEvent();// only run one event at a time or we can create dead-locks
		}

		public override void Exit()
		{
			Orbital_Host_Application_Quit(handle);
		}
		
		private static void Update()
		{
			while (updateThreadRunning)
			{
				Thread.Sleep(100);
				for (int i = Window.windows.Count - 1; i >= 0; --i)
				{
					Window.windows[i].Update();
				}
			}
		}
	}
}
