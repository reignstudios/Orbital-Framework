﻿using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Orbital.Host.Carbon
{
	public unsafe sealed class Application : ApplicationBase
	{
		public static IntPtr handle { get; private set; }
		private bool exit;

		public Application()
		{
			// TODO
		}
		
		public override void Dispose()
		{
			// TODO
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
