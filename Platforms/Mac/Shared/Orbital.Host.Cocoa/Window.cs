using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.Cocoa
{
	public unsafe sealed class Window : WindowBase
	{
		[DllImport(Native.lib)]
		private static extern IntPtr Orbital_Host_Window_Create();

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_Init(IntPtr window, int width, int height, WindowType type, WindowStartupPosition center, int fullscreenIsOverlay);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_Dispose(IntPtr window);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_SetTitle(IntPtr window, char* title, int titleLength);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_Show(IntPtr window);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_Close(IntPtr window);

		[DllImport(Native.lib)]
		private static extern int Orbital_Host_Window_IsClosed(IntPtr window);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_GetSize(IntPtr window, out int width, out int height);

		[DllImport(Native.lib)]
		private static extern void Orbital_Host_Window_SetWindowClosedCallback(IntPtr window, IntPtr funcPtr);
		
		private static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;
		
		public IntPtr handle { get; private set; }

		private delegate void NativeClosedCallbackDef();
		private NativeClosedCallbackDef NativeClosedCallback;
		private IntPtr NativeClosedCallbackFuncPtr;

		public Window(string title, int width, int height, WindowType type, WindowStartupPosition startupPosition, bool fullscreenIsOverlay)
		{
			// create and init native window
			handle = Orbital_Host_Window_Create();
			Orbital_Host_Window_Init(handle, width, height, type, startupPosition, fullscreenIsOverlay ? 1 : 0);

			// bind window closed callback
			NativeClosedCallback = NativeClosed;
			NativeClosedCallbackFuncPtr = Marshal.GetFunctionPointerForDelegate(NativeClosedCallback);
			Orbital_Host_Window_SetWindowClosedCallback(handle, NativeClosedCallbackFuncPtr);

			// set title
			fixed (char* titlePtr = title)
			{
				Orbital_Host_Window_SetTitle(handle, titlePtr, title.Length);
			}

			// show
			Orbital_Host_Window_Show(handle);

			// track window
			_windows.Add(this);
		}

		private void NativeClosed()
		{
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Window_Dispose(handle);
				handle = IntPtr.Zero;
			}
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

		public override void Close()
		{
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Window_Close(handle);
				Orbital_Host_Window_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override bool IsClosed()
		{
			if (handle == IntPtr.Zero) return true;
			return Orbital_Host_Window_IsClosed(handle) != 0;
		}

		public override Size2 GetSize()
		{
			Size2 result;
			Orbital_Host_Window_GetSize(handle, out result.width, out result.height);
			return result;
		}
	}
}
