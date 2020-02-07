using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class Texture2D : Texture2DBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		public Texture2D(Device device, TextureMode mode)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Texture.Orbital_Video_D3D12_Texture_Create(device.handle, mode);
		}

		public unsafe bool Init(TextureFormat format, int width, int height, byte[] data)
		{
			this.width = width;
			this.height = height;
			fixed (byte* dataPtr = data)
			{
				uint widthValue = (uint)width;
				uint heightValue = (uint)height;
				uint depthValue = 1;
				return Texture.Orbital_Video_D3D12_Texture_Init(handle, format, TextureType_NativeInterop._2D, 1, &widthValue, &heightValue, &depthValue, &dataPtr) != 0;
			}
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

		public override object GetManagedHandle()
		{
			return this;
		}
	}
}
