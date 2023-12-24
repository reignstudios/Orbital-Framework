using Orbital.Numerics;

namespace Orbital.Host.X11
{
	public sealed class Window : WindowBase
	{
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public IntPtr handle { get; private set; }
		private bool isClosed;

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(size.width, size.height, type, startupPosition);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(width, height, type, startupPosition);
		}

		private unsafe void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			int sc = X11.XDefaultScreen(Application.dc);
			handle = X11.XCreateSimpleWindow(Application.dc, X11.XRootWindow(Application.dc, sc), 100, 50, (uint)width, (uint)height, 0, X11.XBlackPixel(Application.dc, sc), X11.XWhitePixel(Application.dc, sc));
			
			// handle input
			//X11.XSelectInput(Application.dc, handle, X11.ExposureMask | X11.KeyPressMask | X11.KeyReleaseMask | X11.ButtonPressMask | X11.ButtonReleaseMask);
			
			// Enable Capture of close box
			var normalHint = X11.XInternAtom(Application.dc, "WM_NORMAL_HINTS", false);
			var deleteHint = X11.XInternAtom(Application.dc, "WM_DELETE_WINDOW", false);
			var hints = new IntPtr[] { normalHint, deleteHint };
			X11.XSetWMProtocols(Application.dc, handle, hints, hints.Length);
			
			// window properties
			var sizeHints = new X11.XSizeHints();
			var flags = X11.XSizeHintsFlags.PPosition;
			
			if (type != WindowType.Standard)// window cannot resize
			{
				flags |= X11.XSizeHintsFlags.PMinSize | X11.XSizeHintsFlags.PMaxSize;
				sizeHints.min_width = sizeHints.max_width = width;
				sizeHints.min_height = sizeHints.max_height = height;
			}
			
			sizeHints.flags = (IntPtr)flags;
			X11.XSetNormalHints(Application.dc, handle, &sizeHints);
			
			// center screen
			/*if (startupPosition == WindowStartupPosition.CenterScreen)
			{
				var display = Displays.GetPrimaryDisplay();
				X11.XMoveWindow(Application.dc, handle, (display.width - width) / 2, (display.height - height) / 2);
			}*/
			
			const uint XA_ATOM = 4;
			const int PropModeReplace = 0;
			var propertyType = X11.XInternAtom(Application.dc, "_NET_WM_WINDOW_TYPE", false);
			var centerHint = X11.XInternAtom(Application.dc, "_NET_WM_WINDOW_TYPE_SPLASH", false);
			X11.XChangeProperty(Application.dc, handle, propertyType, (IntPtr)XA_ATOM, 32, PropModeReplace, (byte*)&centerHint, 1);

			// track window
			_windows.Add(this);
		}

		public override void Dispose()
		{
			Close();
		}

		public override IntPtr GetHandle()
		{
			return handle;
		}

		public override object GetManagedHandle()
		{
			return this;
		}

		public override void SetTitle(string title)
		{
			X11.XStoreName(Application.dc, handle, title);
		}

		public override void Show()
		{
			X11.XMapWindow(Application.dc, handle);
		}

		public override void Close()
		{
			isClosed = true;
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				X11.XDestroyWindow(Application.dc, handle);
				handle = IntPtr.Zero;
			}
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public unsafe override Size2 GetSize()
		{
			X11.XWindowAttributes a;
			if (X11.XGetWindowAttributes(Application.dc, handle, &a) != 0) return new Size2(a.width, a.height);
			else return new Size2();
		}
	}
}
