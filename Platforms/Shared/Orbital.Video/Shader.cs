using System;

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
		GS
	}

	public enum ShaderDataType
	{
		/// <summary>
		/// Binary contains CS2X metadata header
		/// </summary>
		CS2X,

		/// <summary>
		/// Native shader without any metadata header
		/// </summary>
		Native
	}

	public abstract class ShaderBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Gets shader type
		/// </summary>
		public abstract ShaderType GetType();
	}
}
