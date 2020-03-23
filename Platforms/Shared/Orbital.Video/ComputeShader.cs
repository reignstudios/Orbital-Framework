namespace Orbital.Video
{
	public struct ComputeShaderConstantBuffer
	{
		/// <summary>
		/// Register index of constant buffer
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Ordered variables
		/// </summary>
		public ShaderVariable[] variables;
	}

	public struct ComputeShaderTexture
	{
		/// <summary>
		/// Register index of the texture
		/// </summary>
		public int registerIndex;
	}

	public struct ComputeShaderReadWriteBuffer
	{
		/// <summary>
		/// Register index of the texture
		/// </summary>
		public int registerIndex;
	}

	public struct ComputeShaderDesc
	{
		public ComputeShaderConstantBuffer[] constantBuffers;
		public ComputeShaderTexture[] textures;
		public ShaderSampler[] samplers;
		public ComputeShaderReadWriteBuffer[] readWriteBuffers;
	}

	public abstract class ComputeShaderBase : ShaderBase
	{
		public ComputeShaderBase(DeviceBase device)
		: base(device, ShaderType.CS)
		{}
	}
}
