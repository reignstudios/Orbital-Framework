using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Host.API
{
	public struct WindowDesc_X11
	{
		public bool boarderlessIsSplash;
	}
	
	public struct WindowDesc_Wayland
	{
		public Wayland.WindowContentType contentType;
	}
	
	public struct WindowDesc_GTK3
	{
		public bool boarderlessIsSplash;
	}
	
	public struct WindowDesc
	{
		public string title;
		public int width, height;
		public WindowType type;
		public WindowStartupPosition startupPosition;
		public WindowDesc_X11 x11;
		public WindowDesc_Wayland wayland;
		public WindowDesc_GTK3 gtk3;
	}
	
	public sealed class Window : WindowBase
	{
		public WindowBase window { get; private set; }
		
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;
		
		public Window(WindowDesc desc)
		{
			switch (Application.api)
			{
				case ApplicationAPI.X11:
					window = new X11.Window(desc.title, desc.width, desc.height, desc.type, desc.startupPosition, desc.x11.boarderlessIsSplash);
					break;
				
				case ApplicationAPI.Wayland:
					window = new Wayland.Window(desc.title, desc.width, desc.height, desc.type, desc.startupPosition, desc.wayland.contentType);
					break;

				case ApplicationAPI.Mir:
					window = new Mir.Window(desc.title);
					break;

				case ApplicationAPI.GTK3:
					window = new GTK3.Window(desc.title, desc.width, desc.height, desc.type, desc.startupPosition, desc.gtk3.boarderlessIsSplash);
					break;
				
				case ApplicationAPI.GTK4:
					window = new GTK4.Window(desc.title, desc.width, desc.height, desc.type, desc.startupPosition);
					break;
			}

			// track window
			_windows.Add(this);
		}

		public override void Dispose()
		{
			window.Dispose();
		}

		public override IntPtr GetHandle()
		{
			return window.GetHandle();
		}

		public override object GetManagedHandle()
		{
			return this;
		}

		public override void Close()
		{
			_windows.Remove(this);
			window.Close();
		}

		public override bool IsClosed()
		{
			return window.IsClosed();
		}

		public override Size2 GetSize()
		{
			return window.GetSize();
		}
	}
}
