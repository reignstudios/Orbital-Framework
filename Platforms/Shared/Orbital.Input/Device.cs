using System;

namespace Orbital.Input
{
	public abstract class DeviceBase : IDisposable
	{
		public InstanceBase instance { get; private set; }

		public DeviceBase(InstanceBase instance)
		{
			this.instance = instance;
		}

		public abstract void Dispose();

		/// <summary>
		/// Update device & controller states
		/// </summary>
		public abstract void Update();
	}
}
