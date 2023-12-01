using System;
using System.Windows.Interop;
using Orbital.Numerics;
using Orbital.OS.Win;

using WPFWindow = System.Windows.Window;

namespace Orbital.Host.WPF
{
	public sealed class Window : WindowBase
	{
		public readonly WPFWindow window;
		private IntPtr handle;
		private bool isClosed;

		public Window(WPFWindow window)
		{
			this.window = window;
		}

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition)
		{
			window = new WPFWindow();
			Init(size.width, size.height, type, startupPosition);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			window = new WPFWindow();
			Init(width, height, type, startupPosition);
		}

		private void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			// get native HWND handle
			handle = new WindowInteropHelper(window).EnsureHandle();
			if (handle == IntPtr.Zero) throw new Exception("WindowInteropHelper HWND failed");

			// set form type
			switch (type)
			{
				case WindowType.Tool:
					window.ResizeMode = System.Windows.ResizeMode.NoResize;
					break;

				case WindowType.Borderless:
					window.WindowStyle = System.Windows.WindowStyle.None;
					break;
			}

			// set form size
			SetSize(width, height);

			// set form startup position
			switch (startupPosition)
			{
				case WindowStartupPosition.CenterScreen:
					window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
					break;
			}

			// watch for close event
			window.Closed += Window_Closed;
		}

		private unsafe void SetSize(int width, int height)
		{
			// get window rect and size
			var rect = new User32.RECT();
			int result = User32.GetWindowRect(handle, &rect);
			if (result == 0) throw new Exception("GetWindowRect failed");
			int rectWidth = rect.right - rect.left;
			int rectHeight = rect.bottom - rect.top;

			// get client rect and size
			var clientRect = new User32.RECT();
			result = User32.GetClientRect(handle, &clientRect);
			if (result == 0) throw new Exception("GetClientRect failed");
			int clientRectWidth = clientRect.right - clientRect.left;
			int clientRectHeight = clientRect.bottom - clientRect.top;

			// increase size based on client side decoration delta
			width = width + (rectWidth - clientRectWidth);
			height = height + (rectHeight - clientRectHeight);

			// apply new adjusted window size
			result = User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, width, height, User32.SWP_NOMOVE);
			if (result == 0) throw new Exception("SetWindowPos failed");
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			isClosed = true;
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
			return window;
		}

		public override void SetTitle(string title)
		{
			window.Title = title;
		}

		public override void Show()
		{
			window.Show();
		}

		public override void Close()
		{
			window.Close();
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

        public override Size2 GetSize()
        {
            return new Size2((int)window.Width, (int)window.Height);
        }
	}
}
