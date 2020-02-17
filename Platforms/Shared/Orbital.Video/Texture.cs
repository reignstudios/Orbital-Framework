﻿using System;

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
		/// <summary>
		/// Let the API choose default non-HDR format
		/// </summary>
		Default,

		/// <summary>
		/// Let the API choose default HDR format
		/// </summary>
		DefaultHDR,

		/// <summary>
		/// 32bit: 8bit for RGBA non-floating point channels
		/// </summary>
		B8G8R8A8,

		/// <summary>
		/// 32bit: 10bit for RGB floating point channels + 2bit for alpha as non-floating point channel
		/// </summary>
		R10G10B10A2,

		/// <summary>
		/// 64bit: 16bit for RGBA floating point channels
		/// </summary>
		R16G16B16A16,

		/// <summary>
		/// 128bit: 32bit for RGBA floating point channels
		/// </summary>
		R32G32B32A32,
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
