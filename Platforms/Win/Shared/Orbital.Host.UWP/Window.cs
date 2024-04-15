using System;
using Orbital.Numerics;

using UWPWindow = Windows.UI.Xaml.Window;

namespace Orbital.Host.UWP
{
	public sealed class Window : WindowBase
	{
		public UWPWindow window { get; private set; }
		private bool isClosed;

		public Window(UWPWindow window)
		{
			this.window = window;
			window.Closed += Window_Closed;
		}

		private void Window_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
		{
			isClosed = true;
		}

		public override void Dispose()
		{
			Close();
		}

		public override IntPtr GetHandle()
		{
			return IntPtr.Zero;
		}

		public override object GetManagedHandle()
		{
			return window;
		}

		public override void Close()
		{
			isClosed = true;
			window.Closed -= Window_Closed;
			window.Close();
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

        public override Size2 GetSize()
        {
			var bounds = window.Bounds;
			return new Size2((int)bounds.Width, (int)bounds.Height);
        }
	}
}
