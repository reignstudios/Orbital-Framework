using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Orbital.OS.Lin;

namespace Orbital.Host.GTK3
{
	public unsafe static class Application
	{
		public static IntPtr context { get; private set; }
		public static IntPtr app { get; private set; }
		private static bool exit;

		/// <summary>
		/// Init Application
		/// </summary>
		/// <param name="appID">Must be in the 'com.company.product' format</param>
		public static void Init(string appID)
		{
			LibraryResolver.Init(Assembly.GetExecutingAssembly());
			
			// create app instance
			fixed (byte* appIDPtr = Encoding.ASCII.GetBytes(appID + "\0"))
			{
				app = GTK3.gtk_application_new(appIDPtr, GTK3.GApplicationFlags.G_APPLICATION_FLAGS_NONE);
				if (app == IntPtr.Zero) throw new Exception("Failed to create application instance");
			}
			
			// get context
			context = GTK3.g_main_context_default();
			if (GTK3.g_main_context_acquire(context) == 0) throw new Exception("Failed to aquire context");
			
			// register app
			GTK3.GError *error = null;
			if (GTK3.g_application_register(app, IntPtr.Zero, &error) == 0)
			{
				// TODO: capture error messages
				throw new Exception("Failed to register app");
			}
		}

		public static void Shutdown()
		{
			// close all windows
			for (int i = Window._windows.Count - 1; i >= 0; --i)
			{
				Window._windows[i].Close();
			}
			
			// release context
			if (context != IntPtr.Zero)
			{
				GTK3.g_main_context_release(context);
				context = IntPtr.Zero;
			}
			
			// release app handle
			if (app != IntPtr.Zero)
			{
				GTK3.g_object_unref(app);
				app = IntPtr.Zero;
			}
		}

		public static int Run(string[] args)
		{
			if (args == null) return GTK3.g_application_run(app, 0, null);
			
			// buffer args into heap
			byte** argsPtr = stackalloc byte*[args.Length];
			for (int i = 0; i != args.Length; ++i)
			{
				byte[] arg = Encoding.ASCII.GetBytes(args[i]);
				fixed (byte* argPtr = arg)
				{
					argsPtr[i] = (byte*)Marshal.AllocHGlobal(arg.Length);
					Buffer.MemoryCopy(argPtr, argsPtr[i], arg.Length, arg.Length);
				}
			}
			
			// run app
			int result = GTK3.g_application_run(app, args.Length, argsPtr);
			
			// release arg heap
			for (int i = 0; i != args.Length; ++i)
			{
				Marshal.FreeHGlobal((IntPtr)argsPtr[i]);
			}
			
			// finish
			return result;
		}

		public static void Run(Window window)
		{
			while (!exit && !window.IsClosed())
			{
				GTK3.g_main_context_iteration(context, 1);
			}
		}

		public static void RunEvents()
		{
			GTK3.g_main_context_iteration(context, 0);
		}

		public static void Exit()
		{
			if (!exit) GTK3.g_application_quit(app);
			exit = true;
		}
	}
}
