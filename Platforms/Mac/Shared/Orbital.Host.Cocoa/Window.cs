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
		private static extern void Orbital_Host_Window_Init(IntPtr window);

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Dispose(IntPtr window);

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Show(IntPtr window);

		[DllImport(Application.lib)]
		private static extern void Orbital_Host_Window_Close(IntPtr window);

		[DllImport(Application.lib)]
		private static extern int Orbital_Host_Window_IsClosed(IntPtr window);
		
		private static List<Window> windows = new List<Window>();
		public IntPtr handle { get; private set; }

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
			handle = Orbital_Host_Window_Create();
			Orbital_Host_Window_Init(handle);

			// track window
			windows.Add(this);
		}

		public override void Dispose()
		{
			Close();
			Orbital_Host_Window_Dispose(handle);
		}

		public override IntPtr GetHandle()
		{
			return handle;
		}

		public override object GetManagedHandle()
		{
			return this;
		}

		public override void SetTitle(string title)
		{
			// TODO
		}

		public override void Show()
		{
			Orbital_Host_Window_Show(handle);
		}

		public override void Close()
		{
			Orbital_Host_Window_Close(handle);
		}

		public override bool IsClosed()
		{
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

		public override Size2 GetSize(WindowSizeType type)
		{
			// TODO
			return new Size2();
		}

		public override void SetSize(int width, int height, WindowSizeType type)
		{
			// TODO
		}
	}
}
