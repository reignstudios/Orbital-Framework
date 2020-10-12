using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class ComputeState : ComputeStateBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;

		public ComputeState(Device device)
		: base(device)
		{
			throw new NotImplementedException();
		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
