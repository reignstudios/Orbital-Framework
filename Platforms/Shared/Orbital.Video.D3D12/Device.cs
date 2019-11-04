using System;

namespace Orbital.Video.D3D12
{
	public sealed class Device : DeviceBase
	{
		public Device(DeviceType type)
		: base(type)
		{
			
		}

		public override void BeginFrame()
		{
			throw new NotImplementedException();
		}

		public override void EndFrame()
		{
			throw new NotImplementedException();
		}
	}
}
