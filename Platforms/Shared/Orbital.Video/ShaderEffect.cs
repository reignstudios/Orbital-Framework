using System;
using System.IO;
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

	public abstract class ShaderEffectBase : IDisposable
	{
		public int constantBufferCount { get; protected set; }
		public int textureCount { get; protected set; }

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

		protected abstract bool InitFinish(ref ShaderEffectDesc desc);
		protected abstract bool CreateShader(byte[] data, ShaderType type);
	}
}
