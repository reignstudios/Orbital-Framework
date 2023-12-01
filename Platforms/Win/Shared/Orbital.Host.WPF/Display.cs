using System;
using System.Runtime.InteropServices;
using System.Windows;
using Orbital.OS.Win;

using BOOL = System.Int32;
using HDC = System.IntPtr;
using HMONITOR = System.IntPtr;
using LPARAM = System.IntPtr;

namespace Orbital.Host.WPF
{
	public static class Displays
	{
		private static MonitorenumprocDelegate monitorenumprocDelegate;
		private static IntPtr monitorenumprocDelegateNativeHandle;

		static Displays()
		{
			monitorenumprocDelegate = new MonitorenumprocDelegate(Monitorenumproc);
			monitorenumprocDelegateNativeHandle = Marshal.GetFunctionPointerForDelegate<MonitorenumprocDelegate>(monitorenumprocDelegate);
		}

		public static Display GetPrimaryDisplay()
		{
			return new Display()
			{
				isPrimary = true,
				width = (int)SystemParameters.PrimaryScreenWidth,
				height = (int)SystemParameters.PrimaryScreenHeight
			};
		}

		#region Get Displays
		unsafe struct GetDisplayData
		{
			public int count;
			public Display* displays;
		}

		public unsafe static Display[] GetDisplays()
		{
			int displayCount = User32.GetSystemMetrics(User32.SM_CMONITORS);
			if (displayCount <= 0) return new Display[0];

			var displays = stackalloc Display[displayCount];
			var getDisplaysData = new GetDisplayData()
			{
				displays = displays
			};

			while (getDisplaysData.count < displayCount && User32.EnumDisplayMonitors(HDC.Zero, null, monitorenumprocDelegateNativeHandle, new LPARAM(&getDisplaysData)) != 0)
			{
				// do nothing...
			}

			var results = new Display[getDisplaysData.count];
			for (int i = 0; i < getDisplaysData.count; ++i)
			{
				results[i] = getDisplaysData.displays[i];
			}
			
			return results;
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate BOOL MonitorenumprocDelegate(HMONITOR hmonitor, HDC hdc, ref Windows.RECT rect, LPARAM data);
		private unsafe static BOOL Monitorenumproc(HMONITOR hMonitor, HDC hdc, ref Windows.RECT rect, LPARAM data)
		{
			var getDisplaysData = (GetDisplayData*)data.ToPointer();
			int count = getDisplaysData->count;

			var display = &getDisplaysData->displays[count];
			display->isPrimary = count == 0;
			display->width = rect.right - rect.left;
			display->height = rect.bottom - rect.top;

			getDisplaysData->count = ++count;
			return 0;
		}
		#endregion
	}
}
