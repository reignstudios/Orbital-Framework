using System;

namespace Orbital.Input
{
	public abstract class InstanceBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Update devices states
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// Gets an array of all possible devices
		/// </summary>
		public abstract DeviceBase[] GetDevices();
	}
}
