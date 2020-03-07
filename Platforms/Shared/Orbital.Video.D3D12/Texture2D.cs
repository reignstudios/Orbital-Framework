using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public class Texture2D : Texture2DBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		public Texture2D(Device device, TextureMode mode)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Texture.Orbital_Video_D3D12_Texture_Create(device.handle, mode);
		}

		public virtual bool Init(int width, int height, TextureFormat format, byte[] data)
		{
			return Init(width, height, format, data, false, MSAALevel.Disabled);
		}

		internal unsafe bool Init(int width, int height, TextureFormat format, byte[] data, bool isRenderTexture, MSAALevel msaaLevel)
		{
			this.msaaLevel = msaaLevel;
			this.width = width;
			this.height = height;
			uint widthValue = (uint)width;
			uint heightValue = (uint)height;
			uint depthValue = 1;
			if (data == null) return Texture.Orbital_Video_D3D12_Texture_Init(handle, format, TextureType_NativeInterop._2D, 1, &widthValue, &heightValue, &depthValue, null, isRenderTexture ? 1 : 0, msaaLevel) != 0;
			fixed (byte* dataPtr = data) return Texture.Orbital_Video_D3D12_Texture_Init(handle, format, TextureType_NativeInterop._2D, 1, &widthValue, &heightValue, &depthValue, &dataPtr, isRenderTexture ? 1 : 0, msaaLevel) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Texture.Orbital_Video_D3D12_Texture_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override IntPtr GetHandle()
		{
			return handle;
		}
	}
}
