using System;
using System.Collections.Generic;
using Orbital.Numerics;
using AppKit;
using ObjCRuntime;
using CoreGraphics;
using Foundation;

namespace Orbital.Host.Mac
{
	public sealed class Window : WindowBase
	{
		sealed class WindowDelegates : NSWindowDelegate
		{
			private Window window;
			
			public WindowDelegates(Window window)
			{
				this.window = window;
			}
			
			public override void WillClose(NSNotification notification)
			{
				window.isClosed = true;
				_windows.Remove(window);
				window.handle.Dispose();
			}
		}
		
		private static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public NSWindow handle { get; private set; }
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
			handle = new NSWindow();
			handle.BackingType = NSBackingStore.Buffered;
			handle.Delegate = new WindowDelegates(this);
			//handle.ReleaseWhenClosed(true);// NOTE: this throws an nonsense exception for some reason
			
			// configure window type
			switch (type)
			{
				case WindowType.Standard:
					handle.StyleMask = NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable;
					break;
				
				case WindowType.Tool:
					handle.StyleMask = NSWindowStyle.Titled | NSWindowStyle.Closable;
					break;
				
				case WindowType.Borderless:
					handle.StyleMask = NSWindowStyle.Borderless;
					break;
				
				case WindowType.Fullscreen:
					throw new NotImplementedException();
					break;
			}

			// set window size
			handle.SetContentSize(new CGSize(width, height));

			// set window position
			if (startupPosition == WindowStartupPosition.CenterScreen)
			{
				handle.Center();
			}
			else// default
			{
				var screenFrame = NSScreen.MainScreen.Frame;
				var screenSize = screenFrame.Size;
				handle.SetFrameTopLeftPoint(new CGPoint(20, screenSize.Height - 40));
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
			return handle.GetHandle();
		}

		public override object GetManagedHandle()
		{
			return handle;
		}

		public override void SetTitle(string title)
		{
			handle.Title = title;
		}

		public override void Show()
		{
			handle.MakeKeyAndOrderFront(null);
		}

		public override void Close()
		{
			isClosed = true;
			_windows.Remove(this);
			handle.Close();
			handle.Dispose();
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Size2 GetSize()
		{
			var size = handle.Frame.Size;
			return new Size2((int)size.Width, (int)size.Height);
		}
	}
}
