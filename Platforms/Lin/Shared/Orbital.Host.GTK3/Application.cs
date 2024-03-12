using System.Text;

namespace Orbital.Host.GTK3
{
	public unsafe static class Application
	{
		public static IntPtr app { get; private set; }
		private static bool exit;
		
		static void Activate(IntPtr app, void* user_data)
		{
			IntPtr window = GTK3.gtk_application_window_new(app);
			
			fixed (byte* titlePtr = Encoding.ASCII.GetBytes("Demo: GTK3\0"))
			{
				GTK3.gtk_window_set_title(window, titlePtr);
			}

			GTK3.gtk_window_set_default_size(window, 200, 200);
			GTK3.gtk_widget_show_all(window);
		}
		
		public static void Init(string appID)
		{
			// create app instance
			fixed (byte* appIDPtr = Encoding.ASCII.GetBytes(appID + "\0"))
			{
				app = GTK3.gtk_application_new(appIDPtr, GTK3.GApplicationFlags.G_APPLICATION_FLAGS_NONE);
				if (app == IntPtr.Zero) throw new Exception("Failed to create application instance");
			}
			
			// activate app
			fixed (byte* activatePtr = Encoding.ASCII.GetBytes("activate\0"))
			{
				GTK3.g_signal_connect(app, activatePtr, GTK3.G_CALLBACK(&Activate), IntPtr.Zero);
			}

			// run
			int status = GTK3.g_application_run(app, 0, null);// TODO: status should be returned from main

			// shutdown
			if (app != IntPtr.Zero)
			{
				GTK3.g_object_unref(app);
			}
		}

		public static void Shutdown()
		{
			// close all windows
			for (int i = Window._windows.Count - 1; i >= 0; --i)
			{
				Window._windows[i].Close();
			}
		}

		public static void Run()
		{
			/*while (!exit)
			{
				X11.XEvent e;
				while (!exit && X11.XNextEvent(dc, &e) >= 0)
				{
					ProcessEvent(e);
				}
			}*/
		}

		public static void Run(Window window)
		{
			/*while (!exit && !window.IsClosed())
			{
				X11.XEvent e;
				while (!exit && !window.IsClosed() && X11.XNextEvent(dc, &e) >= 0)
				{
					ProcessEvent(e);
				}
			}*/
		}

		public static void RunEvents()
		{
			/*while (X11.XPending(dc) != 0)
			{
				X11.XEvent e;
				X11.XPeekEvent(dc, &e);
				ProcessEvent(e);
				X11.XNextEvent(dc, &e);
			}*/
		}

		public static void Exit()
		{
			exit = true;
		}
	}
}
