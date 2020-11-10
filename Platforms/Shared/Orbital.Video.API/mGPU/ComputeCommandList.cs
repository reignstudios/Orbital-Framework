namespace Orbital.Video.API.mGPU
{
	public sealed class ComputeCommandList : ComputeCommandListBase
	{
		public readonly Device deviceMGPU;

		public ComputeCommandList(Device device)
		: base(device)
		{
			deviceMGPU = device;
		}

		public override void Dispose()
		{
			throw new System.NotImplementedException();
		}

		public override void Execute()
		{
			throw new System.NotImplementedException();
		}

		public override void ExecuteComputeShader(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
		{
			throw new System.NotImplementedException();
		}

		public override void Finish()
		{
			throw new System.NotImplementedException();
		}

		public bool Init()
		{
			return false;
		}

		public override void SetComputeState(ComputeStateBase computeState)
		{
			throw new System.NotImplementedException();
		}

		public override void Start(int nodeIndex)
		{
			throw new System.NotImplementedException();
		}
	}
}
