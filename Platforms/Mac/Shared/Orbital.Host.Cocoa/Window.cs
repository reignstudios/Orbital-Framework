using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.Cocoa
{
	public unsafe sealed class Window : WindowBase
	{
		private static List<Window> windows = new List<Window>();

		public IntPtr handle { get; private set; }
		private bool isClosed;

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
			// TODO

			// track window
			windows.Add(this);
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

		public override void SetTitle(string title)
		{
			// TODO
		}

		public override void Show()
		{
			// TODO
		}

		public override void Hide()
		{
			// TODO
		}

		public override void Close()
		{
			// TODO
		}

		public override bool IsVisible()
		{
			return false;// TODO
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Point2 GetPosition()
		{
			// TODO
			return new Point2();
		}

		public override void SetPosition(Point2 position)
		{
			SetPosition(position.x, position.y);
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

		public override void SetSize(Size2 size, WindowSizeType type)
		{
			SetSize(size.width, size.height, type);
		}

		public override void SetSize(int width, int height, WindowSizeType type)
		{
			// TODO
		}
	}
}
