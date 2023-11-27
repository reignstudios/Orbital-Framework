using System;

namespace Orbital.Host
{
	/// <summary>
	/// Application orientation
	/// </summary>
	[Flags]
	public enum ApplicationOrientation
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

	public abstract class ApplicationBase : IDisposable
	{
		public virtual void Dispose() {}
		
		/// <summary>
		/// Returns pointer to platform specific native handle
		/// </summary>
		public virtual IntPtr GetHandle() => IntPtr.Zero;

		/// <summary>
		/// Returns pointer to platform specific managed handle
		/// </summary>
		public virtual object GetManagedHandle() => this;
		
		public abstract void Run();
		public abstract void Run(WindowBase window);
		public abstract void RunEvents();
		public abstract void Exit();
	}
}
