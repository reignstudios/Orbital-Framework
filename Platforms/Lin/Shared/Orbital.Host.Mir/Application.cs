using System;
using System.Runtime.InteropServices;
using System.Reflection;
using Orbital.OS.Lin;

namespace Orbital.Host.Mir
{
	public static class Application
	{
		public static void Init()
		{
			LibraryResolver.Init(Assembly.GetExecutingAssembly());
			
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
			// TODO
		}
	}
}
