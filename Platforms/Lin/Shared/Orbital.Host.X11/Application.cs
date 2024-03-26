using System.Reflection;
using Orbital.OS.Lin;

namespace Orbital.Host.X11
{
	public unsafe static class Application
	{
		public static IntPtr dc {get; private set;}
		private static bool exit;
		
		public static void Init()
		{
			LibraryResolver.Init(Assembly.GetExecutingAssembly());
			
			dc = X11.XOpenDisplay(null);
			if (dc == IntPtr.Zero) throw new Exception("XOpenDisplay failed");
		}

		public static void Shutdown()
		{
			// close all windows
			for (int i = Window._windows.Count - 1; i >= 0; --i)
			{
				Window._windows[i].Close();
			}
			
			// close xServer
			X11.XCloseDisplay(dc);
		}

		private static void ProcessEvent(X11.XEvent e)
		{
			int keyCode = (int)e.xkey.keycode;
			switch (e.type)
			{
				case (X11.Expose):
					foreach (var window in Window._windows)
					{
						if (window.handle == e.xexpose.window)
						{
							// TODO: window shown
							break;
						}
					}
					break;
						
				case (X11.ClientMessage):
					X11.XSendEvent(dc, e.xclient.window, 0, 0, &e);
					foreach (var window in Window._windows)
					{
						if (window.handle == e.xclient.window)
						{
							window.Dispose();
							break;
						}
					}
					return;
						
				/*case (X11.KeyPress):
					theEvent.Type = ApplicationEventTypes.KeyDown;
					theEvent.KeyCode = keyCode;
					break;

				case (X11.KeyRelease):
					theEvent.Type = ApplicationEventTypes.KeyUp;
					theEvent.KeyCode = keyCode;
					break;

				case (X11.ButtonPress):
					if (keyCode == 1) theEvent.Type = ApplicationEventTypes.LeftMouseDown;
					if (keyCode == 2) theEvent.Type = ApplicationEventTypes.MiddleMouseDown;
					if (keyCode == 3) theEvent.Type = ApplicationEventTypes.RightMouseDown;
					if (keyCode == 4)
					{
						theEvent.Type = ApplicationEventTypes.ScrollWheel;
						theEvent.ScrollWheelVelocity = 1;
					}
					if (keyCode == 5)
					{
						theEvent.Type = ApplicationEventTypes.ScrollWheel;
						theEvent.ScrollWheelVelocity = -1;
					}
					theEvent.KeyCode = keyCode;
					break;

				case (X11.ButtonRelease):
					if (keyCode == 1) theEvent.Type = ApplicationEventTypes.LeftMouseUp;
					if (keyCode == 2) theEvent.Type = ApplicationEventTypes.MiddleMouseUp;
					if (keyCode == 3) theEvent.Type = ApplicationEventTypes.RightMouseUp;
					if (keyCode == 4)
					{
						theEvent.Type = ApplicationEventTypes.ScrollWheel;
						theEvent.ScrollWheelVelocity = 1;
					}
					if (keyCode == 5)
					{
						theEvent.Type = ApplicationEventTypes.ScrollWheel;
						theEvent.ScrollWheelVelocity = -1;
					}
					theEvent.KeyCode = keyCode;
					break;*/
			}
		}

		public static void Run()
		{
			while (!exit)
			{
				X11.XEvent e;
				while (!exit && X11.XNextEvent(dc, &e) >= 0)
				{
					ProcessEvent(e);
				}
			}
		}

		public static void Run(Window window)
		{
			while (!exit && !window.IsClosed())
			{
				X11.XEvent e;
				while (!exit && !window.IsClosed() && X11.XNextEvent(dc, &e) >= 0)
				{
					ProcessEvent(e);
				}
			}
		}

		public static void RunEvents()
		{
			while (X11.XPending(dc) != 0)
			{
				X11.XEvent e;
				X11.XPeekEvent(dc, &e);
				ProcessEvent(e);
				X11.XNextEvent(dc, &e);
			}
		}

		public static void Exit()
		{
			exit = true;
		}
	}
}
