using System.Text;
using Orbital.Numerics;

namespace Orbital.Host.GTK3
{
	public sealed class Window : WindowBase
	{
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public IntPtr handle { get; private set; }
		private bool isClosed;

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition, bool borderlessIsSplash)
		{
			Init(size.width, size.height, type, startupPosition, borderlessIsSplash);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition, bool borderlessIsSplash)
		{
			Init(width, height, type, startupPosition, borderlessIsSplash);
		}

		private unsafe void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition, bool borderlessIsSplash)
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
			
		}

		public override void Show()
		{
			
		}

		public override void Close()
		{
			isClosed = true;
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				// TODO
				handle = IntPtr.Zero;
			}
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public unsafe override Size2 GetSize()
		{
			return new Size2();// TODO
		}
	}
}
