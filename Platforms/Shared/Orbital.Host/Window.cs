using System;
using Orbital.Numerics;

namespace Orbital.Host
{
	/// <summary>
	/// Meaning of window size
	/// </summary>
	public enum WindowSizeType
	{
		/// <summary>
		/// Size includes client side decorations
		/// </summary>
		ClientDecorations,

		/// <summary>
		/// Size only represents usable area
		/// </summary>
		WorkingArea
	}

	/// <summary>
	/// Window creation type presets
	/// </summary>
	public enum WindowType
	{
		/// <summary>
		/// Whatever the host defaults to
		/// </summary>
		Default,

		/// <summary>
		/// No resize, minimize or maximize if possible on host
		/// </summary>
		Tool,

		/// <summary>
		/// No client side decorations if possible on host
		/// </summary>
		Popup
	}

	/// <summary>
	/// Window creation / startup position
	/// </summary>
	public enum WindowStartupPosition
	{
		/// <summary>
		/// Let the host decide
		/// </summary>
		Default,

		/// <summary>
		/// Use position parameters if possible
		/// </summary>
		Custom,

		/// <summary>
		/// Center hosts screen if possible
		/// </summary>
		CenterScreen
	}

	public abstract class WindowBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Returns pointer to platform specific native handle
		/// </summary>
		public abstract IntPtr GetHandle();

		/// <summary>
		/// Returns pointer to platform specific managed handle
		/// </summary>
		public abstract object GetManagedHandle();

		public virtual void SetTitle(string title) { }
		public virtual void Show() { }
		public virtual void Close() { }
		public virtual bool IsClosed() => false;
		
		public virtual Point2 GetPosition() => Point2.zero;
		public void SetPosition(Point2 position) => SetPosition(position.x, position.y);
		public abstract void SetPosition(int x, int y);
		
		public abstract Size2 GetSize(WindowSizeType type);
		public void SetSize(Size2 size, WindowSizeType type) => SetSize(size.width, size.height, type);
		public abstract void SetSize(int width, int height, WindowSizeType type);
	}
}
