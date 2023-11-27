using System;
using System.Threading;
using System.Runtime.InteropServices;
using AppKit;
using ObjCRuntime;

namespace Orbital.Host.Microsoft
{
	public sealed class Application : ApplicationBase
	{
		public static NSApplication handle { get; private set; }
		private bool exit;

		public Application()
		{
			// TODO
		}

		public override void Dispose()
		{
			// TODO
		}
		
		public override IntPtr GetHandle()
		{
			return handle.GetHandle();
		}

		public override object GetManagedHandle()
		{
			return handle;
		}

		public override void Run()
		{
			// TODO
		}

		public override void Run(WindowBase window)
		{
			// TODO
		}

		public override void RunEvents()
		{
			// TODO
		}

		public override void Exit()
		{
			exit = true;
		}
	}
}
