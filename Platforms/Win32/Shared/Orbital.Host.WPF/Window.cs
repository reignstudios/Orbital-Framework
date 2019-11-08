using WPFWindow = System.Windows.Window;

using System;
using System.Windows.Interop;
using Orbital.Numerics;
using System.Runtime.InteropServices;

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

		public Window(Point2 position, Size2 size, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			window = new WPFWindow();
			Init(position.x, position.y, size.width, size.height, sizeType, type, startupPosition);
		}

		public Window(int x, int y, int width, int height, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			window = new WPFWindow();
			Init(x, y, width, height, sizeType, type, startupPosition);
		}

		private void Init(int x, int y, int width, int height, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
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

				case WindowType.Popup:
					window.WindowStyle = System.Windows.WindowStyle.None;
					break;
			}

			// set form size
			SetSize(width, height, sizeType);

			// set form startup position
			switch (startupPosition)
			{
				case WindowStartupPosition.Custom:
					window.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
					SetPosition(x, y);
					break;

				case WindowStartupPosition.CenterScreen:
					window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
					break;
			}

			// watch for close event
			window.Closed += Window_Closed;
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

		public override void SetTitle(string title)
		{
			window.Title = title;
		}

		public override void Show()
		{
			window.Show();
		}

		public override void Hide()
		{
			window.Hide();
		}

		public override void Close()
		{
			window.Close();
		}

		public override bool IsVisible()
		{
			return window.IsVisible;
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Point2 GetPosition()
		{
			return new Point2((int)window.Left, (int)window.Top);
		}

		public override void SetPosition(Point2 position)
		{
			SetPosition(position.x, position.y);
		}

		public override void SetPosition(int x, int y)
		{
			window.Left = x;
			window.Top = y;
		}

		public override Size2 GetSize(WindowSizeType type)
		{
			if (type == WindowSizeType.WorkingArea) return new Size2((int)window.Width, (int)window.Height);
			else return new Size2((int)window.Width, (int)window.Height);
		}

		public override void SetSize(Size2 size, WindowSizeType type)
		{
			SetSize(size.width, size.height, type);
		}

		public override void SetSize(int width, int height, WindowSizeType type)
		{
			if (type == WindowSizeType.WorkingArea)
			{
				// get window rect and size
				RECT rect = new RECT();
				int result = GetWindowRect(handle, ref rect);
				if (result == 0) throw new Exception("GetWindowRect failed");
				int rectWidth = rect.right - rect.left;
				int rectHeight = rect.bottom - rect.top;

				// get client rect and size
				RECT clientRect = new RECT();
				result = GetClientRect(handle, ref clientRect);
				if (result == 0) throw new Exception("GetClientRect failed");
				int clientRectWidth = clientRect.right - clientRect.left;
				int clientRectHeight = clientRect.bottom - clientRect.top;

				// increase size based on client side decoration delta
				width = width + (rectWidth - clientRectWidth);
				height = height + (rectHeight - clientRectHeight);

				// apply new adjusted window size
				result = SetWindowPos(handle, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE);
				if (result == 0) throw new Exception("SetWindowPos failed");
			}
			else
			{
				window.Width = width;
				window.Height = height;
			}
		}

		#region Native Helpers
		[StructLayout(LayoutKind.Sequential)]
		struct RECT
		{
			public int left, top, right, bottom;
		}

		private const string lib = "User32.dll";

		[DllImport(lib, EntryPoint = "GetWindowRect")]
		private extern static int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

		[DllImport(lib, EntryPoint = "GetClientRect")]
		private extern static int GetClientRect(IntPtr hWnd, ref RECT lpRect);

		[DllImport(lib, EntryPoint = "SetWindowPos")]
		private extern static int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		private const int SWP_NOMOVE = 0x0002;
		#endregion
	}
}
