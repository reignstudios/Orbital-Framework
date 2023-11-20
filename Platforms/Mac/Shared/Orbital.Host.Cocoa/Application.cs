using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Orbital.Host.Cocoa
{
	public sealed class Application : ApplicationBase
	{
		public const string lib = "../../../../../Native/Orbital.Host.Cocoa.Native/Host/DerivedData/Host/Build/Products/Debug/libHost.dylib";

		[DllImport(lib)]
		private static extern IntPtr Orbital_Host_Application_Create();

		[DllImport(lib)]
		private static extern void Orbital_Host_Application_Init(IntPtr application);

		[DllImport(lib)]
		private static extern void Orbital_Host_Application_Run();

		[DllImport(lib)]
		private static extern int Orbital_Host_Application_RunEvents(IntPtr application);
		
		[DllImport(lib)]
		private static extern void Orbital_Host_Application_Quit();
		
		public static IntPtr handle { get; private set; }

		static Application()
		{
			handle = Orbital_Host_Application_Create();
			Orbital_Host_Application_Init(handle);
		}

		public override void Run()
		{
			Orbital_Host_Application_Run();
		}

		public override void Run(WindowBase window)
		{
			while (!window.IsClosed())
			{
				//Console.WriteLine("IsClosed 1");
				RunEvents();
				//Console.WriteLine("IsClosed 2");
			}
			Console.WriteLine("QUIT");
		}

		public override void RunEvents()
		{
			while (Orbital_Host_Application_RunEvents(handle) != 0)
			{
				Console.WriteLine("Orbital_Host_Application_RunEvents");
			}
			Thread.Sleep(1);// TODO: remove 
		}

		public override void Exit()
		{
			Orbital_Host_Application_Quit();
		}
	}
}
