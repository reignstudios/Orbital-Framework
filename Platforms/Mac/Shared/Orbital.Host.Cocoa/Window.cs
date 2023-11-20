using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.Cocoa
{
	public sealed class Window : WindowBase
	{
		[DllImport(Application.lib)]
		private static extern IntPtr Orbital_Host_Window_Create();

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Init(IntPtr window, int x, int y, int width, int height, int center);

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Dispose(IntPtr window);

		[DllImport(Application.lib)]
		private static extern unsafe void Orbital_Host_Window_SetTitle(IntPtr window, char* title, int titleLength);

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Show(IntPtr window);

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Close(IntPtr window);

		[DllImport(Application.lib)]
		private static extern int Orbital_Host_Window_IsClosed(IntPtr window);
		
		private static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;
		
		public IntPtr handle { get; private set; }

		public Window(Point2 position, Size2 size, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(position.x, position.y, size.width, size.height, type, startupPosition);
		}

		public Window(int x, int y, int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(x, y, width, height, type, startupPosition);
		}

		private void Init(int x, int y, int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			handle = Orbital_Host_Window_Create();
			int center = (startupPosition == WindowStartupPosition.CenterScreen) ? 1 : 0;
			Orbital_Host_Window_Init(handle, x, y, width, height, center);
			_windows.Add(this);
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

		public override unsafe void SetTitle(string title)
		{
			fixed (char* titlePtr = title)
			{
				Orbital_Host_Window_SetTitle(handle, titlePtr, title.Length);
			}
		}

		public override void Show()
		{
			Orbital_Host_Window_Show(handle);
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

		public override Point2 GetPosition()
		{
			// TODO
			return new Point2();
		}

		public override void SetPosition(int x, int y)
		{
			// TODO
		}

		public override Size2 GetSize()
		{
			// TODO
			return new Size2();
		}

		public override void SetSize(int width, int height)
		{
			// TODO
		}

		internal void Update()
		{
			if (IsClosed())
			{
				_windows.Remove(this);
				Dispose();
			}
		}
	}
}
