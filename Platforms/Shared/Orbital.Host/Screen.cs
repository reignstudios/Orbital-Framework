using System;
using System.Collections.Generic;

namespace Orbital.Host
{
	public struct Screen
	{
		public bool isPrimary;
		public int x, y, width, height;
	}

	public abstract class ScreensBase : IDisposable
	{
		public virtual void Dispose() {}
		public abstract Screen[] GetScreens();
	}
}
