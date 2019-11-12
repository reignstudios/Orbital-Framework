using System;

namespace Orbital.Video
{
	public class AdapterInfo
	{
		public readonly int index;
		public readonly string name;

		public AdapterInfo(int index, string name)
		{
			this.index = index;
			this.name = name;
		}
	}

	public abstract class InstanceBase : IDisposable
	{
		public abstract void Dispose();
		public abstract bool QuerySupportedAdapters(bool allowSoftwareAdapters, out AdapterInfo[] adapters);
	}
}
