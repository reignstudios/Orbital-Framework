using Orbital.Video;
using System.Runtime.InteropServices;
using System;

namespace Orbital.Video.API.mGPU
{
	public sealed class RenderState : RenderStateBase
	{
		public readonly Device deviceMGPU;
		public RenderStateBase[] states { get; private set; }

		public RenderState(Device device, RenderStateBase[] states)
		: base(device)
		{
			deviceMGPU = device;
			this.states = states;
		}

		public void Init(RenderStateDesc desc)
		{
			InitBase(ref desc);
		}

		public override void Dispose()
		{
			if (states != null)
			{
				foreach (var state in states)
				{
					if (state != null) state.Dispose();
				}
				states = null;
			}
		}
	}
}
