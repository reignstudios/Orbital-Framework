using System;

namespace Orbital.Input
{
	public abstract class DeviceBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Update device & controller states
		/// </summary>
		public abstract void Update();
	}
}
