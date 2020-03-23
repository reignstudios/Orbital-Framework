using System;
using System.IO;
using System.Collections.ObjectModel;
using Orbital.IO;

namespace Orbital.Video
{
	[Flags]
	public enum ShaderEffectResourceUsage
	{
		/// <summary>
		/// Vertex Shader
		/// </summary>
		VS = 1,

		/// <summary>
		/// Pixel Shader
		/// </summary>
		PS = 2,

		/// <summary>
		/// Hull Shader
		/// </summary>
		HS = 4,

		/// <summary>
		/// Domain Shader
		/// </summary>
		DS = 8,

		/// <summary>
		/// Geometry Shader
		/// </summary>
		GS = 16,

		/// <summary>
		/// Used in all types
		/// </summary>
		All = VS | PS | HS | DS | GS
	}

	public struct ShaderEffectConstantBuffer
	{
		/// <summary>
		/// Register index of constant buffer
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Shader types the constant buffer is used in
		/// </summary>
		public ShaderEffectResourceUsage usage;

		/// <summary>
		/// Ordered variables
		/// </summary>
		public ShaderVariable[] variables;
	}

	public struct ShaderEffectTexture
	{
		/// <summary>
		/// Register index of the texture
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Shader types the texture is used in
		/// </summary>
		public ShaderEffectResourceUsage usage;
	}

	public struct ShaderEffectReadWriteBuffer
	{
		/// <summary>
		/// Register index of the texture
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Shader types the texture is used in
		/// </summary>
		public ShaderEffectResourceUsage usage;
	}

	public struct ShaderEffectDesc
	{
		public ShaderEffectConstantBuffer[] constantBuffers;
		public ShaderEffectTexture[] textures;
		public ShaderSampler[] samplers;
		public ShaderEffectReadWriteBuffer[] readWriteBuffers;
	}

	public abstract class ShaderEffectBase : IDisposable
	{
		public readonly DeviceBase device;
		public int constantBufferCount { get; protected set; }
		public int textureCount { get; protected set; }
		public ReadOnlyCollection<ShaderConstantBufferMapping> constantBufferMappings { get; private set; }

		public ShaderEffectBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		public bool Init(Stream stream, ShaderSamplerAnisotropy anisotropyOverride)
		{
			// read shader effect desc
			var desc = new ShaderEffectDesc();// TODO: read/create ShaderEffectDesc.
			if (anisotropyOverride != ShaderSamplerAnisotropy.Default && desc.samplers != null)
			{
				for (int i = 0; i != desc.samplers.Length; ++i) desc.samplers[i].anisotropy = anisotropyOverride;
			}

			// read shaders
			var reader = new StreamBinaryReader(stream);
			int shaderCount = stream.ReadByte();
			for (int i = 0; i != shaderCount; ++i)
			{
				// read shader type
				var type = (ShaderType)stream.ReadByte();

				// read shader data
				int shaderSize = reader.ReadInt32();
				var shaderData = new byte[shaderSize];
				int read = stream.Read(shaderData, 0, shaderSize);
				if (read < shaderSize) throw new Exception("End of file reached");

				// create shader
				if (!CreateShader(shaderData, type)) return false;
			}

			return InitFinish(ref desc);
		}

		protected virtual bool InitFinish(ref ShaderEffectDesc desc)
		{
			if (desc.constantBuffers != null) constantBufferCount = desc.constantBuffers.Length;
			if (desc.textures != null) textureCount = desc.textures.Length;

			// calculate constant buffer variable mappings
			if (desc.constantBuffers != null && desc.constantBuffers.Length != 0)
			{
				var constantBufferVariables = new ShaderVariable[desc.constantBuffers.Length][];
				for (int i = 0; i != desc.constantBuffers.Length; ++i) constantBufferVariables[i] = desc.constantBuffers[i].variables;
				ShaderBase.CalculateContantBufferVariableMappings(constantBufferVariables, out var mappings);
				constantBufferMappings = mappings;
			}

			return true;
		}

		protected abstract bool CreateShader(byte[] data, ShaderType type);

		public bool FindVariable(string name, out ShaderVariableMapping variable)
		{
			return ShaderBase.FindVariable(constantBufferMappings, name, out variable);
		}
	}
}
