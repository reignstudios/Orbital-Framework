using System;
using Orbital.Numerics;

namespace Orbital.Host
{	
	/// <summary>
	/// Window creation type presets
	/// </summary>
	public enum WindowType
	{
		/// <summary>
		/// Whatever the host defaults to (resize, minimize & maximize enabled if available)
		/// </summary>
		Standard,

		/// <summary>
		/// No resize, minimize or maximize if possible on host
		/// </summary>
		Tool,

		/// <summary>
		/// No client side decorations if possible on host
		/// </summary>
		Borderless
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
		/// Center on screen if possible
		/// </summary>
		CenterScreen
	}

	/// <summary>
	/// Window orientation
	/// </summary>
	[Flags]
	public enum WindowOrientation
	{
		All = 0,

		/// <summary>
		/// Landscape left
		/// </summary>
		Landscape = 1,

		/// <summary>
		/// Landscape right
		/// </summary>
		LandscapeFlipped = 2,

		/// <summary>
		/// Portrait up
		/// </summary>
		Portrait = 4,

		/// <summary>
		/// Portrait down
		/// </summary>
		PortraitFlipped = 8
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
		public abstract Size2 GetSize();
	}
}
