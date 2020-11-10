namespace Orbital.Video.API.mGPU
{
	public sealed class RenderPass : RenderPassBase
	{
		public readonly Device deviceMGPU;

		public RenderPass(Device device)
		: base(device)
		{
			deviceMGPU = device;
		}

		public override void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}
