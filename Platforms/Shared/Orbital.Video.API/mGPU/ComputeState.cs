namespace Orbital.Video.API.mGPU
{
	public sealed class ComputeState : ComputeStateBase
	{
		public readonly Device deviceMGPU;
		public ComputeStateBase[] states { get; private set; }

		public ComputeState(Device device, ComputeStateBase[] states)
		: base(device)
		{
			deviceMGPU = device;
			this.states = states;
		}

		public void Init(ComputeStateDesc desc)
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
