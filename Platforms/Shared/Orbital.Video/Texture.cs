using System;

namespace Orbital.Video
{
	public enum TextureMode
	{
		/// <summary>
		/// Memory will be optimized for GPU only use
		/// </summary>
		GPUOptimized
	}

	public enum TextureFormat
	{
		Default,
		DefaultHDR,
		B8G8R8A8,
		R10G10B10A2
	}

	public struct TextureMipLevel2D
	{
		public int width, height;
		public byte[] data;
	}

	public abstract class TextureBase : IDisposable
	{
		public readonly DeviceBase device;

		public TextureBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		/// <summary>
		/// Returns pointer to platform specific native handle
		/// </summary>
		public abstract IntPtr GetHandle();

		/// <summary>
		/// Returns pointer to platform specific managed handle
		/// </summary>
		public abstract object GetManagedHandle();
	}

	public abstract class Texture2DBase : TextureBase
	{
		public int width { get; protected set; }
		public int height { get; protected set; }

		public Texture2DBase(DeviceBase device)
		: base(device)
		{}
	}
}
