using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.OS.Win;

using BOOL = System.Int32;
using HDC = System.IntPtr;
using HMONITOR = System.IntPtr;
using LPARAM = System.IntPtr;

namespace Orbital.Host.Win
{
	public sealed class Screens : ScreensBase
	{
		private static MonitorenumprocDelegate monitorenumprocDelegate;
		private static IntPtr monitorenumprocDelegateNativeHandle;

		static Screens()
		{
			monitorenumprocDelegate = new MonitorenumprocDelegate(Monitorenumproc);
			monitorenumprocDelegateNativeHandle = Marshal.GetFunctionPointerForDelegate<MonitorenumprocDelegate>(monitorenumprocDelegate);
		}

		public Screens()
		{
			if (Application.hdc == HDC.Zero) throw new Exception("Screen can only be created after Application");
		}

		#region GetScreens
		unsafe struct GetScreensData
		{
			public int count;
			public Screen* screens;
		}

		public unsafe override Screen[] GetScreens()
		{
			int screenCount = User32.GetSystemMetrics(User32.SM_CMONITORS);
			if (screenCount <= 0) return new Screen[0];

			var screens = stackalloc Screen[screenCount];
			var getScreensData = new GetScreensData()
			{
				screens = screens
			};

			while (getScreensData.count < screenCount && User32.EnumDisplayMonitors(HDC.Zero, null, monitorenumprocDelegateNativeHandle, new LPARAM(&getScreensData)) != 0)
			{
				// do nothing...
			}

			var results = new Screen[getScreensData.count];
			for (int i = 0; i < getScreensData.count; ++i)
			{
				results[i] = getScreensData.screens[i];
			}
			
			return results;
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate BOOL MonitorenumprocDelegate(HMONITOR hmonitor, HDC hdc, ref Windows.RECT rect, LPARAM data);
		private unsafe static BOOL Monitorenumproc(HMONITOR hMonitor, HDC hdc, ref Windows.RECT rect, LPARAM data)
		{
			var getScreensData = (GetScreensData*)data.ToPointer();
			int count = getScreensData->count;

			var screen = &getScreensData->screens[count];
			screen->isPrimary = count == 0;
			screen->x = rect.left;
			screen->y = rect.top;
			screen->width = rect.right - rect.left;
			screen->height = rect.bottom - rect.top;

			getScreensData->count = ++count;
			return 0;
		}
		#endregion
	}
}
