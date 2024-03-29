﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.Wayland
{
	public sealed class Window : WindowBase
	{
		private static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public IntPtr handle { get; private set; }
		private bool isClosed;

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(size.width, size.height, type, startupPosition);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(width, height, type, startupPosition);
		}

		private void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			// TODO

			// track window
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

		public override void SetTitle(string title)
		{
			// TODO
		}

		public override void Show()
		{
			// TODO
		}

		public override void Close()
		{
			_windows.Remove(this);
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Size2 GetSize()
		{
			// TODO
			return new Size2();
		}
	}
}
