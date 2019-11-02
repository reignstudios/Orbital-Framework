using System;
using System.Threading;
using System.Runtime.InteropServices;

using BOOL = System.Int32;
using WORD = System.UInt16;
using DWORD = System.UInt32;
using UINT = System.UInt32;
using HANDLE = System.IntPtr;
using HWND = System.IntPtr;
using WPARAM = System.IntPtr;
using LPARAM = System.IntPtr;
using LRESULT = System.IntPtr;
using HINSTANCE = System.IntPtr;
using LPSTR = System.IntPtr;

namespace Orbital.Host.Win32
{
	public unsafe sealed class Application : ApplicationBase
	{
		public static HINSTANCE hInstance { get; private set; }
		public static int nCmdShow { get; private set; }
		private bool exit;

		static Application()
		{
			// get hInstance
			if (hInstance == HINSTANCE.Zero)
			{
				#if CS2X
				hInstance = Marshal.GetHINSTANCE();
				#else
				hInstance = Marshal.GetHINSTANCE(typeof(Window).Module);
				#endif
			}

			// get nCmdShow
			if (nCmdShow == 0)
			{
				const int STARTF_USESHOWWINDOW = 0x00000001;
				const int SW_SHOWDEFAULT = 10;
				var info = new STARTUPINFOA();
				GetStartupInfoA(&info);
				if ((info.dwFlags & STARTF_USESHOWWINDOW) == 0) nCmdShow = SW_SHOWDEFAULT;
				else nCmdShow = info.wShowWindow;
			}
		}

		public override void Run()
		{
			var msg = new MSG();
			while (exit)
			{
				while (GetMessageA(&msg, HANDLE.Zero, 0, 0) != 0)
				{
					TranslateMessage(&msg);
					DispatchMessageA(&msg);
				}

				Thread.Sleep(1);
			}
		}

		public override void Run(WindowBase window)
		{
			var windowAbstraction = (Window)window;
			var msg = new MSG();
			while (GetMessageA(&msg, windowAbstraction.hWnd, 0, 0) != 0)
			{
				TranslateMessage(&msg);
				DispatchMessageA(&msg);
			}
		}

		public override void RunEvents()
		{
			const uint PM_REMOVE = 0x0001;
			var msg = new MSG();
			while (PeekMessageA(&msg, HANDLE.Zero, 0, 0, PM_REMOVE) != 0)
			{
				TranslateMessage(&msg);
				DispatchMessageA(&msg);
			}
		}

		public override void Exit()
		{
			exit = true;
		}

		#region Native Helpers
		[StructLayout(LayoutKind.Sequential)]
		unsafe struct STARTUPINFOA
		{
			public DWORD cb;
			public LPSTR lpReserved;
			public LPSTR lpDesktop;
			public LPSTR lpTitle;
			public DWORD dwX;
			public DWORD dwY;
			public DWORD dwXSize;
			public DWORD dwYSize;
			public DWORD dwXCountChars;
			public DWORD dwYCountChars;
			public DWORD dwFillAttribute;
			public DWORD dwFlags;
			public WORD wShowWindow;
			public WORD cbReserved2;
			public IntPtr lpReserved2;
			public HANDLE hStdInput;
			public HANDLE hStdOutput;
			public HANDLE hStdError;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct POINT
		{
			public int x, y;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct MSG
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

		private const string kernalLib = "Kernel32.dll";
		private const string user32Lib = "User32.dll";

		[DllImport(kernalLib, EntryPoint = "GetStartupInfoA")]
		private static extern void GetStartupInfoA(STARTUPINFOA* lpStartupInfo);

		[DllImport(user32Lib, EntryPoint = "GetMessageA")]
		private static extern BOOL GetMessageA(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax);

		[DllImport(user32Lib, EntryPoint = "PeekMessageA")]
		private static extern BOOL PeekMessageA(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg);

		[DllImport(user32Lib, EntryPoint = "TranslateMessage")]
		private static extern BOOL TranslateMessage(MSG* lpMsg);

		[DllImport(user32Lib, EntryPoint = "DispatchMessageA")]
		private static extern LRESULT DispatchMessageA(MSG* lpMsg);
		#endregion
	}
}
