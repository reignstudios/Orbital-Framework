using System;

namespace Orbital.Video
{
	public enum TextureMode
	{
		/// <summary>
		/// Memory will be optimized for GPU only use
		/// </summary>
		GPUOptimized,

		/// <summary>
		/// Memory will be frequently written to by CPU
		/// </summary>
		Write,

		/// <summary>
		/// Memory will be frequently read from the CPU
		/// </summary>
		Read
	}

	public enum TextureFormat
	{
		Default,
		DefaultHDR,
		B8G8R8A8,
		R10G10B10A2
	}

	public abstract class Texture2DBase : IDisposable
	{
		public abstract void Dispose();
	}
}
