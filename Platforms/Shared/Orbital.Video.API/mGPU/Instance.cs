using Orbital.Host;
using System;
using System.IO;

namespace Orbital.Video.API.mGPU
{
	public sealed class Instance : InstanceBase
	{
		public InstanceBase[] instances { get; private set; }

		public Instance(InstanceBase[] instances)
		{
			this.instances = instances;
		}

		public override void Dispose()
		{
			if (instances != null)
			{
				foreach (var instance in instances)
				{
					if (instance != null) instance.Dispose();
				}
				instances = null;
			}
		}

		public override unsafe bool QuerySupportedAdapters(bool allowSoftwareAdapters, out AdapterInfo[] adapters)
		{
			throw new NotImplementedException();
		}
	}
}
