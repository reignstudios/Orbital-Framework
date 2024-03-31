﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.Wayland
{
	public unsafe sealed class Window : WindowBase
	{
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern IntPtr Orbital_Host_Wayland_Window_Create(IntPtr app);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern int Orbital_Host_Wayland_Window_Init(IntPtr window, int width, int height, byte* appID);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Window_Dispose(IntPtr window);
		
		[DllImport(Application.lib, ExactSpelling = true)]
		private static extern void Orbital_Host_Wayland_Window_SetTitle(IntPtr window, byte* title);
		
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
			//System.Threading.Thread.Sleep(20 * 1000);
			handle = Orbital_Host_Wayland_Window_Create(Application.handle);
			fixed (byte* appIDPtr = Application.appIDData)
			{
				if (Orbital_Host_Wayland_Window_Init(handle, width, height, appIDPtr) == 0)
				{
					throw new Exception("Failed: Orbital_Host_Wayland_Window_Init");
				}
			}

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
			var titleData = Encoding.UTF8.GetBytes(title);
			fixed (byte* titlePtr = titleData)
			{
				Orbital_Host_Wayland_Window_SetTitle(handle, titlePtr);
			}
		}

		public override void Show()
		{
			// TODO
		}

		public override void Close()
		{
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				Orbital_Host_Wayland_Window_Dispose(handle);
				handle = IntPtr.Zero;
			}
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
