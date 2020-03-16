using System;
using System.Collections.ObjectModel;

namespace Orbital.Video
{
	public enum ShaderType
	{
		/// <summary>
		/// Vertex Shader
		/// </summary>
		VS,

		/// <summary>
		/// Pixel Shader
		/// </summary>
		PS,

		/// <summary>
		/// Hull Shader
		/// </summary>
		HS,

		/// <summary>
		/// Domain Shader
		/// </summary>
		DS,

		/// <summary>
		/// Geometry Shader
		/// </summary>
		GS,

		/// <summary>
		/// Compute Shader
		/// </summary>
		CS
	}

	public enum ShaderVariableType
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

	public struct ShaderVariable
	{
		/// <summary>
		/// Name of variable
		/// </summary>
		public string name;

		/// <summary>
		/// Data type of variable
		/// </summary>
		public ShaderVariableType type;

		/// <summary>
		/// Number of array elements if applicable
		/// </summary>
		public int elements;
	}

	public enum ShaderSamplerFilter
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

	public enum ShaderSamplerAnisotropy
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

	public enum ShaderSamplerAddress
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

	public enum ShaderComparisonFunction
	{
		/// <summary>
		/// Never pass the comparison / disable comparision function
		/// </summary>
		Never,

		/// <summary>
		/// Always pass the comparison
		/// </summary>
		Always,

		/// <summary>
		/// If the source data is equal to the destination data, the comparison passes
		/// </summary>
		Equal,

		/// <summary>
		/// If the source data is not equal to the destination data, the comparison passes
		/// </summary>
		NotEqual,

		/// <summary>
		/// If the source data is less than the destination data, the comparison passes
		/// </summary>
		LessThan,

		/// <summary>
		/// If the source data is less than or equal to the destination data, the comparison passes
		/// </summary>
		LessThanOrEqual,

		/// <summary>
		/// If the source data is greater than the destination data, the comparison passes
		/// </summary>
		GreaterThan,

		/// <summary>
		/// If the source data is greater than or equal to the destination data, the comparison passes
		/// </summary>
		GreaterThanOrEqual
	}

	public struct ShaderSampler
	{
		/// <summary>
		/// Register index of the sampler
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Texture sampler filter
		/// </summary>
		public ShaderSamplerFilter filter;

		/// <summary>
		/// Anisotropy texture filtering
		/// </summary>
		public ShaderSamplerAnisotropy anisotropy;

		/// <summary>
		/// Texture address mode
		/// </summary>
		public ShaderSamplerAddress addressU, addressV, addressW;

		/// <summary>
		/// Use with special shader sampler method to compare against custom value.
		/// Useful for PCF shadows.
		/// </summary>
		public ShaderComparisonFunction comparisonFunction;
	}

	public class ShaderVariableMapping
	{
		/// <summary>
		/// Name of variable
		/// </summary>
		public readonly string name;

		/// <summary>
		/// Data type of variable
		/// </summary>
		public readonly ShaderVariableType type;

		/// <summary>
		/// Number of array elements if applicable
		/// </summary>
		public readonly int elements;

		/// <summary>
		/// Offset in bytes
		/// </summary>
		public readonly int offset;

		/// <summary>
		/// Create new shader variable
		/// </summary>
		/// <param name="name">Name of variable</param>
		/// <param name="type">Data type of variable</param>
		/// <param name="elements">Number of array elements if applicable</param>
		/// <param name="offset">Offset in bytes</param>
		public ShaderVariableMapping(string name, ShaderVariableType type, int elements, int offset)
		{
			this.name = name;
			this.type = type;
			this.elements = elements;
			this.offset = offset;
		}
	}

	public class ShaderConstantBufferMapping
	{
		/// <summary>
		/// Size of the buffer with alignment padding
		/// </summary>
		public readonly int size;

		/// <summary>
		/// Variable mappings
		/// </summary>
		public readonly ReadOnlyCollection<ShaderVariableMapping> variables;

		public ShaderConstantBufferMapping(int size, ShaderVariableMapping[] variables)
		{
			this.size = size;
			this.variables = new ReadOnlyCollection<ShaderVariableMapping>(variables);
		}

		public bool FindVariable(string name, out ShaderVariableMapping variable)
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

	public abstract class ShaderBase : IDisposable
	{
		public readonly DeviceBase device;
		public readonly ShaderType type;

		public ShaderBase(DeviceBase device, ShaderType type)
		{
			this.device = device;
			this.type = type;
		}

		public abstract void Dispose();

		internal static void CalculateContantBufferVariableMappings(ShaderVariable[][] constantBufferVariables, out ReadOnlyCollection<ShaderConstantBufferMapping> constantBufferMappings)
		{
			int offset = 0, dwordIndex = 0;
			bool firstVariableProcessed = false;
			var constantBufferMappingsArray = new ShaderConstantBufferMapping[constantBufferVariables.Length];
			for (int i = 0; i != constantBufferVariables.Length; ++i)
			{
				var variables = constantBufferVariables[i];
				if (variables == null) throw new ArgumentException("Constant buffers cannot be empty");

				var variablesMapping = new ShaderVariableMapping[variables.Length];
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
					variablesMapping[v] = new ShaderVariableMapping(variable.name, variable.type, variable.elements, offset);
					firstVariableProcessed = true;
					offset += stride;
					dwordIndex += dwordCount;
				}
				constantBufferMappingsArray[i] = new ShaderConstantBufferMapping(offset, variablesMapping);
			}
			constantBufferMappings = new ReadOnlyCollection<ShaderConstantBufferMapping>(constantBufferMappingsArray);
		}

		internal static bool FindVariable(ReadOnlyCollection<ShaderConstantBufferMapping> constantBufferMappings, string name, out ShaderVariableMapping variable)
		{
			if (constantBufferMappings == null)
			{
				variable = null;
				return false;
			}

			foreach (var constantBuffer in constantBufferMappings)
			{
				if (constantBuffer.FindVariable(name, out variable)) return true;
			}

			variable = null;
			return false;
		}

		public static int VariableTypeToSrcDWORDCount(ShaderVariableType type)
		{
			switch (type)
			{
				case ShaderVariableType.Float: return 1;
				case ShaderVariableType.Float2: return 2;
				case ShaderVariableType.Float3: return 3;
				case ShaderVariableType.Float4: return 4;

				case ShaderVariableType.Float2x2: return 2 * 2;
				case ShaderVariableType.Float2x3: return 2 * 3;
				case ShaderVariableType.Float2x4: return 2 * 4;

				case ShaderVariableType.Float3x2: return 3 * 2;
				case ShaderVariableType.Float3x3: return 3 * 3;
				case ShaderVariableType.Float3x4: return 3 * 4;

				case ShaderVariableType.Float4x2: return 4 * 2;
				case ShaderVariableType.Float4x3: return 4 * 3;
				case ShaderVariableType.Float4x4: return 4 * 4;

				case ShaderVariableType.Int: return 1;
				case ShaderVariableType.Int2: return 2;
				case ShaderVariableType.Int3: return 3;
				case ShaderVariableType.Int4: return 4;

				case ShaderVariableType.UInt: return 1;
				case ShaderVariableType.UInt2: return 2;
				case ShaderVariableType.UInt3: return 3;
				case ShaderVariableType.UInt4: return 4;
			}
			throw new NotImplementedException();
		}

		public static int VariableTypeToSrcStride(ShaderVariableType type)
		{
			switch (type)
			{
				case ShaderVariableType.Float: return sizeof(float) * 1;
				case ShaderVariableType.Float2: return sizeof(float) * 2;
				case ShaderVariableType.Float3: return sizeof(float) * 3;
				case ShaderVariableType.Float4: return sizeof(float) * 4;

				case ShaderVariableType.Float2x2: return sizeof(float) * 2 * 2;
				case ShaderVariableType.Float2x3: return sizeof(float) * 2 * 3;
				case ShaderVariableType.Float2x4: return sizeof(float) * 2 * 4;

				case ShaderVariableType.Float3x2: return sizeof(float) * 3 * 2;
				case ShaderVariableType.Float3x3: return sizeof(float) * 3 * 3;
				case ShaderVariableType.Float3x4: return sizeof(float) * 3 * 4;

				case ShaderVariableType.Float4x2: return sizeof(float) * 4 * 2;
				case ShaderVariableType.Float4x3: return sizeof(float) * 4 * 3;
				case ShaderVariableType.Float4x4: return sizeof(float) * 4 * 4;

				case ShaderVariableType.Int: return sizeof(int) * 1;
				case ShaderVariableType.Int2: return sizeof(int) * 2;
				case ShaderVariableType.Int3: return sizeof(int) * 3;
				case ShaderVariableType.Int4: return sizeof(int) * 4;

				case ShaderVariableType.UInt: return sizeof(uint) * 1;
				case ShaderVariableType.UInt2: return sizeof(uint) * 2;
				case ShaderVariableType.UInt3: return sizeof(uint) * 3;
				case ShaderVariableType.UInt4: return sizeof(uint) * 4;
			}
			throw new NotImplementedException();
		}

		public static int VariableTypeToDstStride(ShaderVariableType type)
		{
			switch (type)
			{
				case ShaderVariableType.Float: return sizeof(float) * 1;
				case ShaderVariableType.Float2: return sizeof(float) * 2;
				case ShaderVariableType.Float3: return sizeof(float) * 4;// padded
				case ShaderVariableType.Float4: return sizeof(float) * 4;

				case ShaderVariableType.Float2x2: return sizeof(float) * 2 * 2;
				case ShaderVariableType.Float2x3: return sizeof(float) * 2 * 4;// padded
				case ShaderVariableType.Float2x4: return sizeof(float) * 2 * 4;

				case ShaderVariableType.Float3x2: return sizeof(float) * 3 * 2;
				case ShaderVariableType.Float3x3: return sizeof(float) * 3 * 4;// padded
				case ShaderVariableType.Float3x4: return sizeof(float) * 3 * 4;

				case ShaderVariableType.Float4x2: return sizeof(float) * 4 * 2;
				case ShaderVariableType.Float4x3: return sizeof(float) * 4 * 3;
				case ShaderVariableType.Float4x4: return sizeof(float) * 4 * 4;

				case ShaderVariableType.Int: return sizeof(int) * 1;
				case ShaderVariableType.Int2: return sizeof(int) * 2;
				case ShaderVariableType.Int3: return sizeof(int) * 4;// padded
				case ShaderVariableType.Int4: return sizeof(int) * 4;

				case ShaderVariableType.UInt: return sizeof(uint) * 1;
				case ShaderVariableType.UInt2: return sizeof(uint) * 2;
				case ShaderVariableType.UInt3: return sizeof(uint) * 4;// padded
				case ShaderVariableType.UInt4: return sizeof(uint) * 4;
			}
			throw new NotImplementedException();
		}
	}
}
