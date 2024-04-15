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
		Borderless,

		/// <summary>
		/// Overlays other system elements if possible and auto sets its width & height to match the screen.
		/// Startup Position is also ignored
		/// </summary>
		Fullscreen
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

		public virtual void Close() { }
		public virtual bool IsClosed() => false;
		public abstract Size2 GetSize();
	}
}
