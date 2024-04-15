using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;
using Orbital.OS.Win;

using ATOM = System.UInt16;
using DWORD = System.UInt32;
using UINT = System.UInt32;
using HWND = System.IntPtr;
using WPARAM = System.IntPtr;
using LPARAM = System.IntPtr;
using LRESULT = System.IntPtr;
using HDC = System.IntPtr;
using HBRUSH = System.IntPtr;
using LPCSTR = System.IntPtr;
using HMENU = System.IntPtr;


namespace Orbital.Host.Win
{
	public unsafe sealed class Window : WindowBase
	{
		private static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public ATOM atom { get; private set; }
		public HWND hWnd { get; private set; }
		private bool isClosed;

		private static WndProcDelegate wndProcDelegate;
		private static IntPtr wndProcDelegatePtr;

		static Window()
		{
			wndProcDelegate = new WndProcDelegate(WndProc);
			wndProcDelegatePtr = Marshal.GetFunctionPointerForDelegate<WndProcDelegate>(wndProcDelegate);
		}

		public Window(string title, int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			// register window class
			var wcex = new User32.WNDCLASSEXA();
			wcex.cbSize = (UINT)Marshal.SizeOf<User32.WNDCLASSEXA>();
			wcex.style = User32.CS_HREDRAW | User32.CS_VREDRAW;
			wcex.lpfnWndProc = wndProcDelegatePtr;
			wcex.cbClsExtra = 0;
			wcex.cbWndExtra = 0;
			wcex.hInstance = Application.hInstance;
			//wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINDOWSDESKTOPVCPP));
			//wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));
			wcex.hbrBackground = (HBRUSH)(User32.COLOR_WINDOW + 1);

			var guid = Guid.NewGuid();
			string guidString = guid.ToString();
			fixed (char* guidStringPtr = guidString)
			{
				wcex.lpszClassName = (LPCSTR)guidStringPtr;// set class name to guid
				atom = User32.RegisterClassExA(&wcex);
			}

			// create window
			User32.RECT rect;
			int x, y;
			if (startupPosition == WindowStartupPosition.CenterScreen)
			{
				HWND desktop = User32.GetDesktopWindow();
				User32.GetClientRect(desktop, &rect);
				x = (rect.right / 2) - (width / 2);
				y = (rect.bottom / 2) - (height / 2);
			}
			else// default
			{
				x = unchecked((int)User32.CW_USEDEFAULT);
				y = unchecked((int)User32.CW_USEDEFAULT);
			}

			DWORD windowStyle = User32.WS_OVERLAPPEDWINDOW;
			switch (type)
			{
				case WindowType.Tool:
					windowStyle ^= User32.WS_MAXIMIZEBOX;
					windowStyle ^= User32.WS_MINIMIZEBOX;
					windowStyle ^= User32.WS_THICKFRAME;// disable window resize
					break;

				case WindowType.Borderless:
					windowStyle = User32.WS_POPUPWINDOW;
					break;

				case WindowType.Fullscreen:
					windowStyle = User32.WS_POPUP;
					var display = Displays.GetPrimaryDisplay();
					x = y = 0;
					width = display.width;
					height = display.height;
					break;
			}

			byte[] encodedTitle = Encoding.Default.GetBytes(title + '\0');
			fixed (byte* encodedTitlePtr = encodedTitle)
			{
				hWnd = User32.CreateWindowExA(0, (LPCSTR)atom, (LPCSTR)encodedTitlePtr, windowStyle, x, y, width, height, HWND.Zero, HMENU.Zero, Application.hInstance, IntPtr.Zero);
			}
			if (hWnd == HWND.Zero) throw new Exception("CreateWindowExA failed");

			// adjust working area / client size and position
			if (type != WindowType.Fullscreen)
			{
				if (User32.GetWindowRect(hWnd, &rect) == 0) throw new Exception("GetWindowRect failed");
				int rectWidth = rect.right - rect.left;
				int rectHeight = rect.bottom - rect.top;

				User32.RECT clientRect;
				if (User32.GetClientRect(hWnd, &clientRect) == 0) throw new Exception("GetWindowRect failed");
				int clientRectWidth = clientRect.right - clientRect.left;
				int clientRectHeight = clientRect.bottom - clientRect.top;

				int offsetX = (rectWidth - clientRectWidth);
				int offsetY = (rectHeight - clientRectHeight);
				width += offsetX;
				height += offsetY;

				UINT flags = User32.SWP_NOMOVE;
				if (startupPosition == WindowStartupPosition.CenterScreen)
				{
					flags = 0;
					x -= offsetX / 2;
					y -= offsetY;
				}

				if (User32.SetWindowPos(hWnd, HWND.Zero, x, y, width, height, flags) == 0) throw new Exception("SetWindowPos failed");
			}

			// show
			User32.ShowWindow(hWnd, Application.nCmdShow);
			User32.UpdateWindow(hWnd);

			// track window
			_windows.Add(this);
		}

		public override void Dispose()
		{
			Close();
		}

		public override IntPtr GetHandle()
		{
			return hWnd;
		}

		public override object GetManagedHandle()
		{
			return this;
		}

		/*public void SetTitle(string title)// KEEP: for ref this can set title after creation
		{
			byte[] encodedTitle = Encoding.Default.GetBytes(title + '\0');
			fixed (byte* encodedTitlePtr = encodedTitle)
			{
				User32.SetWindowTextA(hWnd, (LPCSTR)encodedTitlePtr);
			}
		}*/

		public override void Close()
		{
			_windows.Remove(this);
			isClosed = true;
			if (hWnd != HWND.Zero)
			{
				User32.DestroyWindow(hWnd);
				hWnd = HWND.Zero;
			}
			atom = 0;
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Size2 GetSize()
		{
			var rect = new User32.RECT();
			if (User32.GetClientRect(hWnd, &rect) == 0) throw new Exception("GetClientRect failed");
			return new Size2(rect.right - rect.left, rect.bottom - rect.top);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate LRESULT WndProcDelegate(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
		private static LRESULT WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
		{
			switch (message)
			{
				case User32.WM_COMMAND:
					break;

				case User32.WM_PAINT:
					{
						User32.PAINTSTRUCT ps;
						HDC hdc = User32.BeginPaint(hWnd, &ps);
						// TODO: Add any drawing code that uses hdc here...
						User32.EndPaint(hWnd, &ps);
					}
					break;

				case User32.WM_DESTROY:
					User32.PostQuitMessage(0);
					for (int i = windows.Count - 1; i >= 0; --i)
					{
						var window = windows[i];
						if (window.hWnd == hWnd)
						{
							window.hWnd = HWND.Zero;
							window.Dispose();
							break;
						}
					}
					break;

				default:
					return User32.DefWindowProcA(hWnd, message, wParam, lParam);
			}
			return LRESULT.Zero;
		}
	}
}
