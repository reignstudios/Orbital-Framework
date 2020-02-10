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

	public enum ShaderEffectVariableType
	{
		Float,
		Float2,
		Float3,
		Float4,

		Float2x2,
		Float2x3,
		Float2x4,

		Float3x2,
		Float3x3,
		Float3x4,

		Float4x2,
		Float4x3,
		Float4x4,

		Int,
		Int2,
		Int3,
		Int4,

		UInt,
		UInt2,
		UInt3,
		UInt4
	}

	public struct ShaderEffectVariable
	{
		/// <summary>
		/// Name of variable
		/// </summary>
		public string name;

		/// <summary>
		/// Data type of variable
		/// </summary>
		public ShaderEffectVariableType type;

		/// <summary>
		/// Number of array elements if applicable
		/// </summary>
		public int elements;
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
		public ShaderEffectVariable[] variables;
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

	public enum ShaderEffectSamplerFilter
	{
		/// <summary>
		/// Will let the API choose
		/// </summary>
		Default,

		/// <summary>
		/// No sub-pixel interpolation
		/// </summary>
		Point,

		/// <summary>
		/// UV Sub-pixel interpolation
		/// </summary>
		Bilinear,

		/// <summary>
		/// UVW Sub-pixel interpolation (requires mip-maps to function)
		/// </summary>
		Trilinear
	}

	public enum ShaderEffectSamplerAnisotropy
	{
		/// <summary>
		/// Will let the API choose
		/// </summary>
		Default = 0,

		/// <summary>
		/// No filtering
		/// </summary>
		X1 = 1,

		/// <summary>
		/// 2x filtering
		/// </summary>
		X2 = 2,

		/// <summary>
		/// 4x filtering
		/// </summary>
		X4 = 4,

		/// <summary>
		/// 8x filtering
		/// </summary>
		X8 = 8,

		/// <summary>
		/// 16x filtering
		/// </summary>
		X16 = 16
	}

	public enum ShaderEffectSamplerAddress
	{
		/// <summary>
		/// Wrap texture UVs (outputUV = inputUV % 1.0)
		/// </summary>
		Wrap,

		/// <summary>
		/// Clamp texture UVs between 0-1
		/// </summary>
		Clamp
	}

	public struct ShaderEffectSampler
	{
		/// <summary>
		/// Register index of the sampler
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Texture sampler filter
		/// </summary>
		public ShaderEffectSamplerFilter filter;

		/// <summary>
		/// Anisotropy texture filtering
		/// </summary>
		public ShaderEffectSamplerAnisotropy anisotropy;

		/// <summary>
		/// Texture address mode
		/// </summary>
		public ShaderEffectSamplerAddress addressU, addressV, addressW;
	}

	public struct ShaderEffectDesc
	{
		public ShaderEffectConstantBuffer[] constantBuffers;
		public ShaderEffectTexture[] textures;
		public ShaderEffectSampler[] samplers;
	}

	public class ShaderEffectVariableMapping
	{
		/// <summary>
		/// Name of variable
		/// </summary>
		public readonly string name;

		/// <summary>
		/// Data type of variable
		/// </summary>
		public readonly ShaderEffectVariableType type;

		/// <summary>
		/// Number of array elements if applicable
		/// </summary>
		public readonly int elements;

		/// <summary>
		/// Offset in bytes
		/// </summary>
		public readonly int offset;

		/// <summary>
		/// Create new shader effect variable
		/// </summary>
		/// <param name="name">Name of variable</param>
		/// <param name="type">Data type of variable</param>
		/// <param name="elements">Number of array elements if applicable</param>
		/// <param name="offset">Offset in bytes</param>
		public ShaderEffectVariableMapping(string name, ShaderEffectVariableType type, int elements, int offset)
		{
			this.name = name;
			this.type = type;
			this.elements = elements;
			this.offset = offset;
		}
	}

	public class ShaderEffectConstantBufferMapping
	{
		/// <summary>
		/// Size of the buffer with alignment padding
		/// </summary>
		public readonly int size;

		/// <summary>
		/// Variable mappings
		/// </summary>
		public readonly ReadOnlyCollection<ShaderEffectVariableMapping> variables;

		public ShaderEffectConstantBufferMapping(int size, ShaderEffectVariableMapping[] variables)
		{
			this.size = size;
			this.variables = new ReadOnlyCollection<ShaderEffectVariableMapping>(variables);
		}

		public bool FindVariable(string name, out ShaderEffectVariableMapping variable)
		{
			foreach (var v in variables)
			{
				if (v.name == name)
				{
					variable = v;
					return true;
				}
			}
			variable = null;
			return false;
		}
	}

	public abstract class ShaderEffectBase : IDisposable
	{
		public readonly DeviceBase device;
		public int constantBufferCount { get; protected set; }
		public int textureCount { get; protected set; }
		public ReadOnlyCollection<ShaderEffectConstantBufferMapping> constantBufferMappings { get; private set; }

		public ShaderEffectBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		public bool Init(Stream stream, ShaderEffectSamplerAnisotropy anisotropyOverride)
		{
			// read shader effect desc
			var desc = new ShaderEffectDesc();// TODO: read/create ShaderEffectDesc.
			if (anisotropyOverride != ShaderEffectSamplerAnisotropy.Default && desc.samplers != null)
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
			if (desc.constantBuffers != null)
			{
				int offset = 0, dwordIndex = 0;
				bool firstVariableProcessed = false;
				var constantBufferMappingsArray = new ShaderEffectConstantBufferMapping[desc.constantBuffers.Length];
				for (int i = 0; i != desc.constantBuffers.Length; ++i)
				{
					var variables = desc.constantBuffers[i].variables;
					if (variables == null) throw new ArgumentException("Constant buffers cannot be empty");

					var variablesMapping = new ShaderEffectVariableMapping[variables.Length];
					for (int v = 0; v != variables.Length; ++v)
					{
						var variable = variables[v];
						int stride = VariableTypeToSrcStride(variable.type);
						int dwordCount = VariableTypeToSrcDWORDCount(variable.type);
						if (variable.elements >= 1)
						{
							stride *= variable.elements;
							dwordCount *= variable.elements;
						}
						if (firstVariableProcessed && dwordCount != 1)
						{
							int alignedIndex = dwordIndex % 4;
							if (dwordCount > alignedIndex)
							{
								int remander = 4 - alignedIndex;
								dwordIndex += remander;
								offset += remander * sizeof(float);
							}
						}
						variablesMapping[v] = new ShaderEffectVariableMapping(variable.name, variable.type, variable.elements, offset);
						firstVariableProcessed = true;
						offset += stride;
						dwordIndex += dwordCount;
					}
					constantBufferMappingsArray[i] = new ShaderEffectConstantBufferMapping(offset, variablesMapping);
				}
				constantBufferMappings = new ReadOnlyCollection<ShaderEffectConstantBufferMapping>(constantBufferMappingsArray);
			}

			return true;
		}

		protected abstract bool CreateShader(byte[] data, ShaderType type);

		public bool FindVariable(string name, out ShaderEffectVariableMapping variable)
		{
			foreach (var constantBuffer in constantBufferMappings)
			{
				if (constantBuffer.FindVariable(name, out variable)) return true;
			}
			variable = null;
			return false;
		}

		public static int VariableTypeToSrcDWORDCount(ShaderEffectVariableType type)
		{
			switch (type)
			{
				case ShaderEffectVariableType.Float: return 1;
				case ShaderEffectVariableType.Float2: return 2;
				case ShaderEffectVariableType.Float3: return 3;
				case ShaderEffectVariableType.Float4: return 4;

				case ShaderEffectVariableType.Float2x2: return 2 * 2;
				case ShaderEffectVariableType.Float2x3: return 2 * 3;
				case ShaderEffectVariableType.Float2x4: return 2 * 4;

				case ShaderEffectVariableType.Float3x2: return 3 * 2;
				case ShaderEffectVariableType.Float3x3: return 3 * 3;
				case ShaderEffectVariableType.Float3x4: return 3 * 4;

				case ShaderEffectVariableType.Float4x2: return 4 * 2;
				case ShaderEffectVariableType.Float4x3: return 4 * 3;
				case ShaderEffectVariableType.Float4x4: return 4 * 4;

				case ShaderEffectVariableType.Int: return 1;
				case ShaderEffectVariableType.Int2: return 2;
				case ShaderEffectVariableType.Int3: return 3;
				case ShaderEffectVariableType.Int4: return 4;

				case ShaderEffectVariableType.UInt: return 1;
				case ShaderEffectVariableType.UInt2: return 2;
				case ShaderEffectVariableType.UInt3: return 3;
				case ShaderEffectVariableType.UInt4: return 4;
			}
			throw new NotImplementedException();
		}

		public static int VariableTypeToSrcStride(ShaderEffectVariableType type)
		{
			switch (type)
			{
				case ShaderEffectVariableType.Float: return sizeof(float) * 1;
				case ShaderEffectVariableType.Float2: return sizeof(float) * 2;
				case ShaderEffectVariableType.Float3: return sizeof(float) * 3;
				case ShaderEffectVariableType.Float4: return sizeof(float) * 4;

				case ShaderEffectVariableType.Float2x2: return sizeof(float) * 2 * 2;
				case ShaderEffectVariableType.Float2x3: return sizeof(float) * 2 * 3;
				case ShaderEffectVariableType.Float2x4: return sizeof(float) * 2 * 4;

				case ShaderEffectVariableType.Float3x2: return sizeof(float) * 3 * 2;
				case ShaderEffectVariableType.Float3x3: return sizeof(float) * 3 * 3;
				case ShaderEffectVariableType.Float3x4: return sizeof(float) * 3 * 4;

				case ShaderEffectVariableType.Float4x2: return sizeof(float) * 4 * 2;
				case ShaderEffectVariableType.Float4x3: return sizeof(float) * 4 * 3;
				case ShaderEffectVariableType.Float4x4: return sizeof(float) * 4 * 4;

				case ShaderEffectVariableType.Int: return sizeof(int) * 1;
				case ShaderEffectVariableType.Int2: return sizeof(int) * 2;
				case ShaderEffectVariableType.Int3: return sizeof(int) * 3;
				case ShaderEffectVariableType.Int4: return sizeof(int) * 4;

				case ShaderEffectVariableType.UInt: return sizeof(uint) * 1;
				case ShaderEffectVariableType.UInt2: return sizeof(uint) * 2;
				case ShaderEffectVariableType.UInt3: return sizeof(uint) * 3;
				case ShaderEffectVariableType.UInt4: return sizeof(uint) * 4;
			}
			throw new NotImplementedException();
		}

		public static int VariableTypeToDstStride(ShaderEffectVariableType type)
		{
			switch (type)
			{
				case ShaderEffectVariableType.Float: return sizeof(float) * 1;
				case ShaderEffectVariableType.Float2: return sizeof(float) * 2;
				case ShaderEffectVariableType.Float3: return sizeof(float) * 4;// padded
				case ShaderEffectVariableType.Float4: return sizeof(float) * 4;

				case ShaderEffectVariableType.Float2x2: return sizeof(float) * 2 * 2;
				case ShaderEffectVariableType.Float2x3: return sizeof(float) * 2 * 4;// padded
				case ShaderEffectVariableType.Float2x4: return sizeof(float) * 2 * 4;

				case ShaderEffectVariableType.Float3x2: return sizeof(float) * 3 * 2;
				case ShaderEffectVariableType.Float3x3: return sizeof(float) * 3 * 4;// padded
				case ShaderEffectVariableType.Float3x4: return sizeof(float) * 3 * 4;

				case ShaderEffectVariableType.Float4x2: return sizeof(float) * 4 * 2;
				case ShaderEffectVariableType.Float4x3: return sizeof(float) * 4 * 3;
				case ShaderEffectVariableType.Float4x4: return sizeof(float) * 4 * 4;

				case ShaderEffectVariableType.Int: return sizeof(int) * 1;
				case ShaderEffectVariableType.Int2: return sizeof(int) * 2;
				case ShaderEffectVariableType.Int3: return sizeof(int) * 4;// padded
				case ShaderEffectVariableType.Int4: return sizeof(int) * 4;

				case ShaderEffectVariableType.UInt: return sizeof(uint) * 1;
				case ShaderEffectVariableType.UInt2: return sizeof(uint) * 2;
				case ShaderEffectVariableType.UInt3: return sizeof(uint) * 4;// padded
				case ShaderEffectVariableType.UInt4: return sizeof(uint) * 4;
			}
			throw new NotImplementedException();
		}
	}
}
