using System;
using System.Runtime.InteropServices;

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
using HMONITOR = System.IntPtr;
using MONITORENUMPROC = System.IntPtr;

namespace Orbital.OS.Win
{
	public static unsafe class User32
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left, top, right, bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PAINTSTRUCT
		{
			public HDC hdc;
			public BOOL fErase;
			public RECT rcPaint;
			public BOOL fRestore;
			public BOOL fIncUpdate;
			public fixed BYTE rgbReserved[32];
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WNDCLASSEXA
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

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x, y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MSG
		{
			public HWND hwnd;
			public UINT message;
			public WPARAM wParam;
			public LPARAM lParam;
			public DWORD time;
			public POINT pt;
			//#ifdef _MAC
			//public DWORD lPrivate;
			//#endif
		}

		public const uint CS_VREDRAW = 0x0001;
		public const uint CS_HREDRAW = 0x0002;
		public const uint COLOR_WINDOW = 5;
		public const uint CW_USEDEFAULT = 0x80000000;
		public const uint WS_OVERLAPPED = 0x00000000;
		public const uint WS_CAPTION = 0x00C00000;
		public const uint WS_SYSMENU = 0x00080000;
		public const uint WS_THICKFRAME = 0x00040000;
		public const uint WS_MINIMIZEBOX = 0x00020000;
		public const uint WS_MAXIMIZEBOX = 0x00010000;
		public const uint WS_POPUP = 0x80000000;
		public const uint WS_BORDER = 0x00800000;
		public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
		public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;

		public const uint SWP_NOSIZE = 0x0001;
		public const uint SWP_NOMOVE = 0x0002;

		public const uint WM_COMMAND = 0x0111;
		public const uint WM_PAINT = 0x000F;
		public const uint WM_DESTROY = 0x0002;

		public const int SM_CXSCREEN = 0;
		public const int SM_CYSCREEN = 1;
		public const int SM_CMONITORS = 80;

		public const string lib = "User32.dll";

		// Application
		[DllImport(lib, EntryPoint = "GetMessageA")]
		public static extern BOOL GetMessageA(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax);

		[DllImport(lib, EntryPoint = "PeekMessageA")]
		public static extern BOOL PeekMessageA(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg);

		[DllImport(lib, EntryPoint = "TranslateMessage")]
		public static extern BOOL TranslateMessage(MSG* lpMsg);

		[DllImport(lib, EntryPoint = "DispatchMessageA")]
		public static extern LRESULT DispatchMessageA(MSG* lpMsg);

		// Windows
		[DllImport(lib, EntryPoint = "RegisterClassExA")]
		public static extern ATOM RegisterClassExA(WNDCLASSEXA* c);

		[DllImport(lib, EntryPoint = "GetDesktopWindow")]
		public static extern HWND GetDesktopWindow();

		[DllImport(lib, EntryPoint = "GetWindowRect")]
		public static extern BOOL GetWindowRect(HWND hWnd, RECT* lpRect);

		[DllImport(lib, EntryPoint = "GetClientRect")]
		public static extern BOOL GetClientRect(HWND hWnd, RECT* lpRect);

		[DllImport(lib, EntryPoint = "SetWindowPos")]
		public static extern BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags);

		[DllImport(lib, EntryPoint = "CreateWindowExA")]
		public static extern HWND CreateWindowExA(DWORD dwExStyle, LPCSTR lpClassName, LPCSTR lpWindowName, DWORD dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, IntPtr lpParam);

		[DllImport(lib, EntryPoint = "SetWindowTextA")]
		public static extern BOOL SetWindowTextA(HWND hWnd, LPCSTR lpString);

		[DllImport(lib, EntryPoint = "ShowWindow")]
		public static extern BOOL ShowWindow(HWND hWnd, int nCmdShow);

		[DllImport(lib, EntryPoint = "UpdateWindow")]
		public static extern BOOL UpdateWindow(HWND hWnd);

		[DllImport(lib, EntryPoint = "CloseWindow")]
		public static extern BOOL CloseWindow(HWND hWnd);

		[DllImport(lib, EntryPoint = "IsWindowVisible")]
		public static extern BOOL IsWindowVisible(HWND hWnd);

		[DllImport(lib, EntryPoint = "DestroyWindow")]
		public static extern BOOL DestroyWindow(HWND hWnd);

		[DllImport(lib, EntryPoint = "BeginPaint")]
		public static extern HDC BeginPaint(HWND hWnd, PAINTSTRUCT* lpPaint);

		[DllImport(lib, EntryPoint = "EndPaint")]
		public static extern BOOL EndPaint(HWND hWnd, PAINTSTRUCT* lpPaint);

		[DllImport(lib, EntryPoint = "PostQuitMessage")]
		public static extern void PostQuitMessage(int nExitCode);

		[DllImport(lib, EntryPoint = "DefWindowProcA")]
		public static extern LRESULT DefWindowProcA(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);

		// Screens
		[DllImport(lib, EntryPoint = "EnumDisplayMonitors")]
		public static extern BOOL EnumDisplayMonitors(HDC hdc, Windows.RECT* lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData);

		[DllImport(lib, EntryPoint = "GetSystemMetrics")]
		public static extern int GetSystemMetrics(int nIndex);
	}
}
