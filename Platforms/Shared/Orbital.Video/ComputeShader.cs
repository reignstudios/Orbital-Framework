namespace Orbital.Video
{
	public abstract class ComputeShaderBase : ShaderBase
	{
		public ComputeShaderBase(DeviceBase device)
		: base(device, ShaderType.CS)
		{
			
		}

		public override void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}
