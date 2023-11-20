using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

using BOOL = System.Int32;
using ATOM = System.UInt16;
using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD = System.UInt32;
using UINT = System.UInt32;
using HANDLE = System.IntPtr;
using HWND = System.IntPtr;
using WPARAM = System.IntPtr;
using LPARAM = System.IntPtr;
using LRESULT = System.IntPtr;
using HDC = System.IntPtr;
using WNDPROC = System.IntPtr;
using HINSTANCE = System.IntPtr;
using HICON = System.IntPtr;
using HCURSOR = System.IntPtr;
using HBRUSH = System.IntPtr;
using LPCSTR = System.IntPtr;
using HMENU = System.IntPtr;

namespace Orbital.Host.Win
{
	public unsafe sealed class Window : WindowBase
	{
		public static IReadOnlyList<Window> windows = new List<Window>();

		public ATOM atom { get; private set; }
		public HWND hWnd { get; private set; }
		private bool isClosed;
		private WndProcDelegate wndProcDelegate;

		public Window(Point2 position, Size2 size, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(position.x, position.y, size.width, size.height, sizeType, type, startupPosition);
		}

		public Window(int x, int y, int width, int height, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(x, y, width, height, sizeType, type, startupPosition);
		}

		private void Init(int x, int y, int width, int height, WindowSizeType sizeType, WindowType type, WindowStartupPosition startupPosition)
		{
			// register window class
			var wcex = new WNDCLASSEXA();
			wcex.cbSize = (UINT)Marshal.SizeOf<WNDCLASSEXA>();
			wcex.style = CS_HREDRAW | CS_VREDRAW;
			wndProcDelegate = new WndProcDelegate(WndProc);
			#if CS2X
			Marshal.GetFunctionPointerForDelegate<WndProcDelegate>(wndProcDelegate, out _, out wcex.lpfnWndProc);
			#else
			wcex.lpfnWndProc = Marshal.GetFunctionPointerForDelegate<WndProcDelegate>(wndProcDelegate);
			#endif
			wcex.cbClsExtra = 0;
			wcex.cbWndExtra = 0;
			wcex.hInstance = Application.hInstance;
			//wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINDOWSDESKTOPVCPP));
			//wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));
			wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);

			var guid = Guid.NewGuid();
			string guidString = guid.ToString();
			fixed (char* guidStringPtr = guidString)
			{
				wcex.lpszClassName = (LPCSTR)guidStringPtr;// set class name to guid
				atom = RegisterClassExA(&wcex);
			}

			// create window
			RECT rect;
			if (startupPosition == WindowStartupPosition.CenterScreen)
			{
				HWND desktop = GetDesktopWindow();
				GetClientRect(desktop, &rect);
				x = (rect.right / 2) - (width / 2);
				y = (rect.bottom / 2) - (height / 2);
			}
			else if (startupPosition == WindowStartupPosition.Default)
			{
				x = unchecked((int)CW_USEDEFAULT);
				y = unchecked((int)CW_USEDEFAULT);
			}

			DWORD windowStyle = WS_OVERLAPPEDWINDOW;
			switch (type)
			{
				case WindowType.Tool:
					windowStyle ^= WS_MAXIMIZEBOX;
					windowStyle ^= WS_MINIMIZEBOX;
					windowStyle ^= WS_THICKFRAME;// disable window resize
					break;

				case WindowType.Popup:
					windowStyle = WS_POPUPWINDOW;
					break;
			}

			byte* title = stackalloc byte[1];
			title[0] = 0;
			hWnd = CreateWindowExA(0, (LPCSTR)atom, (LPCSTR)title, windowStyle, x, y, width, height, HWND.Zero, HMENU.Zero, Application.hInstance, IntPtr.Zero);
			if (hWnd == HWND.Zero) throw new Exception("CreateWindowExA failed");

			// adjust working area / client size and position
			if (sizeType == WindowSizeType.WorkingArea)
			{
				if (GetWindowRect(hWnd, &rect) == 0) throw new Exception("GetWindowRect failed");
				int rectWidth = rect.right - rect.left;
				int rectHeight = rect.bottom - rect.top;

				RECT clientRect;
				if (GetClientRect(hWnd, &clientRect) == 0) throw new Exception("GetWindowRect failed");
				int clientRectWidth = clientRect.right - clientRect.left;
				int clientRectHeight = clientRect.bottom - clientRect.top;

				int offsetX = (rectWidth - clientRectWidth);
				int offsetY = (rectHeight - clientRectHeight);
				width += offsetX;
				height += offsetY;

				UINT flags = SWP_NOMOVE;
				if (startupPosition == WindowStartupPosition.CenterScreen)
				{
					flags = 0;
					x -= offsetX / 2;
					y -= offsetY;
				}

				if (SetWindowPos(hWnd, HWND.Zero, x, y, width, height, flags) == 0) throw new Exception("SetWindowPos failed");
			}

			// track window
			windows.Add(this);
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

		public override void SetTitle(string title)
		{
			byte[] encodedTitle = Encoding.Default.GetBytes(title + '\0');
			fixed (byte* encodedTitlePtr = encodedTitle)
			{
				SetWindowTextA(hWnd, (LPCSTR)encodedTitlePtr);
			}
		}

		public override void Show()
		{
			ShowWindow(hWnd, Application.nCmdShow);
			UpdateWindow(hWnd);
		}

		public override void Hide()
		{
			CloseWindow(hWnd);
		}

		public override void Close()
		{
			windows.Remove(window);
			isClosed = true;
			if (hWnd != HWND.Zero)
			{
				DestroyWindow(hWnd);
				hWnd = HWND.Zero;
			}
			atom = 0;
		}

		public override bool IsVisible()
		{
			return IsWindowVisible(hWnd) != 0;
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Point2 GetPosition()
		{
			var rect = new RECT();
			if (GetWindowRect(hWnd, &rect) == 0) throw new Exception("GetWindowRect failed");
			return new Point2(rect.left, rect.top);
		}

		public override void SetPosition(Point2 position)
		{
			SetPosition(position.x, position.y);
		}

		public override void SetPosition(int x, int y)
		{
			SetWindowPos(hWnd, HWND.Zero, x, y, 0, 0, SWP_NOSIZE);
		}

		public override Size2 GetSize(WindowSizeType type)
		{
			var rect = new RECT();
			if (type == WindowSizeType.WorkingArea)
			{
				if (GetClientRect(hWnd, &rect) == 0) throw new Exception("GetClientRect failed");
			}
			else
			{
				if (GetWindowRect(hWnd, &rect) == 0) throw new Exception("GetWindowRect failed");
			}
			return new Size2(rect.right - rect.left, rect.bottom - rect.top);
		}

		public override void SetSize(Size2 size, WindowSizeType type)
		{
			SetSize(size.width, size.height, type);
		}

		public override void SetSize(int width, int height, WindowSizeType type)
		{
			if (type == WindowSizeType.WorkingArea)
			{
				var rect = new RECT();
				if (GetWindowRect(hWnd, &rect) == 0) return;
				int rectWidth = rect.right - rect.left;
				int rectHeight = rect.bottom - rect.top;

				var clientRect = new RECT();
				if (GetClientRect(hWnd, &clientRect) == 0) return;
				int clientRectWidth = clientRect.right - clientRect.left;
				int clientRectHeight = clientRect.bottom - clientRect.top;

				width += (rectWidth - clientRectWidth);
				height += (rectHeight - clientRectHeight);
			}

			SetWindowPos(hWnd, HWND.Zero, 0, 0, width, height, SWP_NOMOVE);
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate LRESULT WndProcDelegate(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
		private static LRESULT WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
		{
			switch (message)
			{
				case WM_COMMAND:
					break;

				case WM_PAINT:
					{
						PAINTSTRUCT ps;
						HDC hdc = BeginPaint(hWnd, &ps);
						// TODO: Add any drawing code that uses hdc here...
						EndPaint(hWnd, &ps);
					}
					break;

				case WM_DESTROY:
					PostQuitMessage(0);
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
					return DefWindowProcA(hWnd, message, wParam, lParam);
			}
			return LRESULT.Zero;
		}

		#region Native Helpers
		private const uint CS_VREDRAW = 0x0001;
		private const uint CS_HREDRAW = 0x0002;
		private const uint COLOR_WINDOW = 5;
		private const uint CW_USEDEFAULT = 0x80000000;
		private const uint WS_OVERLAPPED = 0x00000000;
		private const uint WS_CAPTION = 0x00C00000;
		private const uint WS_SYSMENU = 0x00080000;
		private const uint WS_THICKFRAME = 0x00040000;
		private const uint WS_MINIMIZEBOX = 0x00020000;
		private const uint WS_MAXIMIZEBOX = 0x00010000;
		private const uint WS_POPUP = 0x80000000;
		private const uint WS_BORDER = 0x00800000;
		private const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
		private const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;

		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;

		private const uint WM_COMMAND = 0x0111;
		private const uint WM_PAINT = 0x000F;
		private const uint WM_DESTROY = 0x0002;

		[StructLayout(LayoutKind.Sequential)]
		struct RECT
		{
			public int left, top, right, bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct PAINTSTRUCT
		{
			public HDC hdc;
			public BOOL fErase;
			public RECT rcPaint;
			public BOOL fRestore;
			public BOOL fIncUpdate;
			public fixed BYTE rgbReserved[32];
		}

		[StructLayout(LayoutKind.Sequential)]
		struct WNDCLASSEXA
		{
			public UINT cbSize;
			/* Win 3.x */
			public UINT style;
			public WNDPROC lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public HINSTANCE hInstance;
			public HICON hIcon;
			public HCURSOR hCursor;
			public HBRUSH hbrBackground;
			public LPCSTR lpszMenuName;
			public LPCSTR lpszClassName;
			/* Win 4.0 */
			public HICON hIconSm;
		}

		private const string user32Lib = "User32.dll";

		[DllImport(user32Lib, EntryPoint = "RegisterClassExA")]
		private static extern ATOM RegisterClassExA(WNDCLASSEXA* c);

		[DllImport(user32Lib, EntryPoint = "GetDesktopWindow")]
		private static extern HWND GetDesktopWindow();

		[DllImport(user32Lib, EntryPoint = "GetWindowRect")]
		private static extern BOOL GetWindowRect(HWND hWnd, RECT* lpRect);

		[DllImport(user32Lib, EntryPoint = "GetClientRect")]
		private static extern BOOL GetClientRect(HWND hWnd, RECT* lpRect);

		[DllImport(user32Lib, EntryPoint = "SetWindowPos")]
		private static extern BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags);

		[DllImport(user32Lib, EntryPoint = "CreateWindowExA")]
		private static extern HWND CreateWindowExA(DWORD dwExStyle, LPCSTR lpClassName, LPCSTR lpWindowName, DWORD dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, IntPtr lpParam);

		[DllImport(user32Lib, EntryPoint = "SetWindowTextA")]
		private static extern BOOL SetWindowTextA(HWND hWnd, LPCSTR lpString);

		[DllImport(user32Lib, EntryPoint = "ShowWindow")]
		private static extern BOOL ShowWindow(HWND hWnd, int nCmdShow);

		[DllImport(user32Lib, EntryPoint = "UpdateWindow")]
		private static extern BOOL UpdateWindow(HWND hWnd);

		[DllImport(user32Lib, EntryPoint = "CloseWindow")]
		private static extern BOOL CloseWindow(HWND hWnd);

		[DllImport(user32Lib, EntryPoint = "IsWindowVisible")]
		private static extern BOOL IsWindowVisible(HWND hWnd);

		[DllImport(user32Lib, EntryPoint = "DestroyWindow")]
		private static extern BOOL DestroyWindow(HWND hWnd);

		[DllImport(user32Lib, EntryPoint = "BeginPaint")]
		private static extern HDC BeginPaint(HWND hWnd, PAINTSTRUCT* lpPaint);

		[DllImport(user32Lib, EntryPoint = "EndPaint")]
		private static extern BOOL EndPaint(HWND hWnd, PAINTSTRUCT* lpPaint);

		[DllImport(user32Lib, EntryPoint = "PostQuitMessage")]
		private static extern void PostQuitMessage(int nExitCode);

		[DllImport(user32Lib, EntryPoint = "DefWindowProcA")]
		private static extern LRESULT DefWindowProcA(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);
		#endregion
	}
}
