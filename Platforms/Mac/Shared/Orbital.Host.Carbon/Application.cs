using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Orbital.Host.Carbon
{
	public static class Application
	{
		public static IntPtr handle { get; private set; }
		private static bool exit;

		public static void Init()
		{
			// TODO
		}
		
		public static void Shutdown()
		{
			// TODO
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
			exit = true;
		}
	}
}
