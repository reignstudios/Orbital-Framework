using System;
using System.IO;
using Orbital.IO;

namespace Orbital.Video
{
	public struct ShaderEffectResource
	{
		/// <summary>
		/// Register index of the resource
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Shader types the resource is used in
		/// </summary>
		public ShaderType[] usedInTypes;
	}

	public enum ShaderEffectSampleFilter
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

	public enum ShaderEffectSampleAddress
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

	public struct ShaderEffectSampler
	{
		/// <summary>
		/// Register index of the sampler
		/// </summary>
		public int registerIndex;

		/// <summary>
		/// Texture sampler filter
		/// </summary>
		public ShaderEffectSampleFilter filter;

		/// <summary>
		/// Texture address mode
		/// </summary>
		public ShaderEffectSampleAddress addressU, addressV, addressW;

		/// <summary>
		/// Anisotropy texture filtering
		/// </summary>
		public ShaderEffectSamplerAnisotropy anisotropy;
	}

	public struct ShaderEffectDesc
	{
		public ShaderEffectResource[] resources;
		public ShaderEffectSampler[] samplers;
	}

	public abstract class ShaderEffectBase : IDisposable
	{
		public abstract void Dispose();

		public bool Init(Stream stream, ShaderEffectSamplerAnisotropy anisotropyOverride)
		{
			// read shader effect desc
			var desc = new ShaderEffectDesc();// TODO: read/create ShaderEffectDesc.
			if (anisotropyOverride != ShaderEffectSamplerAnisotropy.Default && desc.samplers != null)
			{
				for (int i = 0; i != desc.samplers.Length; ++i) desc.samplers[i].anisotropy = anisotropyOverride;
			}

			// 
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
