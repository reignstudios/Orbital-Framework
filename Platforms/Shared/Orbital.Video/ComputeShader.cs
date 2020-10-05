using System.Collections.ObjectModel;

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
		public int constantBufferCount { get; protected set; }
		public int textureCount { get; protected set; }
		public int samplerCount { get; protected set; }
		public int readWriteBufferCount { get; protected set; }
		public ReadOnlyCollection<ShaderConstantBufferMapping> constantBufferMappings { get; private set; }

		public ComputeShaderBase(DeviceBase device)
		: base(device, ShaderType.CS)
		{}

		protected void InitFinish(ref ComputeShaderDesc desc)
		{
			if (desc.constantBuffers != null) constantBufferCount = desc.constantBuffers.Length;
			if (desc.textures != null) textureCount = desc.textures.Length;
			if (desc.samplers != null) samplerCount = desc.samplers.Length;
			if (desc.readWriteBuffers != null) readWriteBufferCount = desc.readWriteBuffers.Length;

			// calculate constant buffer variable mappings
			if (desc.constantBuffers != null && desc.constantBuffers.Length != 0)
			{
				var constantBufferVariables = new ShaderVariable[desc.constantBuffers.Length][];
				for (int i = 0; i != desc.constantBuffers.Length; ++i) constantBufferVariables[i] = desc.constantBuffers[i].variables;
				CalculateContantBufferVariableMappings(constantBufferVariables, out var mappings);
				constantBufferMappings = mappings;
			}
		}
	}
}
