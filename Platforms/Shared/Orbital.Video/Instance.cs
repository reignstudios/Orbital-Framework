using System;

namespace Orbital.Video
{
	public class AdapterInfo
	{
		/// <summary>
		/// Primary adapter is your main video output device
		/// </summary>
		public readonly bool isPrimary;

		/// <summary>
		/// Adapter index
		/// </summary>
		public readonly int index;

		/// <summary>
		/// Manufacture adapter name
		/// </summary>
		public readonly string name;

		/// <summary>
		/// How many mGPU linked-nodes
		/// </summary>
		public readonly int nodeCount;

		/// <summary>
		/// Amount of deticated memory the adapter has in bytes
		/// </summary>
		public readonly ulong dedicatedGPUMemory;

		/// <summary>
		/// CPU memory reserved for the GPU in bytes
		/// </summary>
		public readonly ulong deticatedSystemMemory;

		/// <summary>
		/// Max shared CPU memory the GPU can use in bytes
		/// </summary>
		public readonly ulong sharedSystemMemory;

		/// <summary>
		/// Max amount of deticated ram the GPU can utalize
		/// </summary>
		public readonly ulong maxReservedMemory;

		/// <summary>
		/// Max amount of ram the GPU might be able to utalize
		/// </summary>
		public readonly ulong maxMemory;

		public AdapterInfo
		(
			bool isPrimary,
			int index,
			string name,
			int nodeCount,
			ulong dedicatedGPUMemory,
			ulong deticatedSystemMemory,
			ulong sharedSystemMemory
		)
		{
			this.isPrimary = isPrimary;
			this.index = index;
			this.name = name;
			this.nodeCount = nodeCount;
			this.dedicatedGPUMemory = dedicatedGPUMemory;
			this.deticatedSystemMemory = deticatedSystemMemory;
			this.sharedSystemMemory = sharedSystemMemory;

			if (dedicatedGPUMemory >= 0)
			{
				maxReservedMemory += dedicatedGPUMemory;
				maxMemory += dedicatedGPUMemory;
			}

			if (deticatedSystemMemory >= 0)
			{
				maxReservedMemory += deticatedSystemMemory;
				maxMemory += deticatedSystemMemory;
			}

			if (sharedSystemMemory >= 0)
			{
				maxMemory += sharedSystemMemory;
			}
		}
	}

	public abstract class InstanceBase : IDisposable
	{
		public abstract void Dispose();
		public abstract bool QuerySupportedAdapters(bool allowSoftwareAdapters, out AdapterInfo[] adapters);
	}
}
