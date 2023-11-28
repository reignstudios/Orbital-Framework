using System.Threading;
using System.Runtime.InteropServices;
using Orbital.OS.Win;

using HDC = System.IntPtr;
using HANDLE = System.IntPtr;
using HINSTANCE = System.IntPtr;

namespace Orbital.Host.Win
{
	public unsafe sealed class Application : ApplicationBase
	{
		public static HDC hdc { get; private set; }
		public static HINSTANCE hInstance { get; private set; }
		public static int nCmdShow { get; private set; }
		private bool exit;

		public Application()
		{
			// get hdc
			hdc = Gdi32.CreateCompatibleDC(HDC.Zero);

			// get hInstance
			#if !NET_STANDARD_20
			if (hInstance == HINSTANCE.Zero)
			{
				#if CS2X
				hInstance = Marshal.GetHINSTANCE();
				#else
				hInstance = Marshal.GetHINSTANCE(typeof(Application).Module);
				#endif
			}
			#endif

			// get nCmdShow
			if (nCmdShow == 0)
			{
				const int STARTF_USESHOWWINDOW = 0x00000001;
				const int SW_SHOWDEFAULT = 10;
				var info = new Kernel32.STARTUPINFOA();
				Kernel32.GetStartupInfoA(&info);
				if ((info.dwFlags & STARTF_USESHOWWINDOW) == 0) nCmdShow = SW_SHOWDEFAULT;
				else nCmdShow = info.wShowWindow;
			}
		}

		#if NET_STANDARD_20
		/// <summary>
		/// Pass the argument: Marshal.GetHINSTANCE(typeof(Application).Module)
		/// </summary>
		/// <param name="hInstance">Marshal.GetHINSTANCE(typeof(Application).Module)</param>
		public Application(HINSTANCE hInstance)
		{
			Application.hInstance = hInstance;
		}
		#endif

		public override void Dispose()
		{
			hdc = HDC.Zero;
			hInstance = HINSTANCE.Zero;
			nCmdShow = 0;
		}

		public override void Run()
		{
			var msg = new User32.MSG();
			while (exit)
			{
				while (User32.GetMessageA(&msg, HANDLE.Zero, 0, 0) != 0)
				{
					User32.TranslateMessage(&msg);
					User32.DispatchMessageA(&msg);
				}

				Thread.Sleep(1);
			}
		}

		public override void Run(WindowBase window)
		{
			var windowAbstraction = (Window)window;
			var msg = new User32.MSG();
			while (User32.GetMessageA(&msg, windowAbstraction.hWnd, 0, 0) != 0)
			{
				User32.TranslateMessage(&msg);
				User32.DispatchMessageA(&msg);
			}
		}

		public override void RunEvents()
		{
			const uint PM_REMOVE = 0x0001;
			var msg = new User32.MSG();
			while (User32.PeekMessageA(&msg, HANDLE.Zero, 0, 0, PM_REMOVE) != 0)
			{
				User32.TranslateMessage(&msg);
				User32.DispatchMessageA(&msg);
			}
		}

		public override void Exit()
		{
			exit = true;
		}
	}
}
