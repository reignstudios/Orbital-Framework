using System;
using System.Threading;
using System.Runtime.InteropServices;
using AppKit;
using ObjCRuntime;

namespace Orbital.Host.Microsoft
{
	public static class Application
	{
		public static NSApplication handle { get; private set; }
		private static bool exit;

		static Application()
		{
			// TODO
		}

		public static void Run()
		{
			// TODO
		}

		public static void Run(WindowBase window)
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
