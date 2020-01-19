using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class Texture2D : Texture2DBase
	{
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Texture_Create(IntPtr device, TextureMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_Texture_Init(IntPtr handle, TextureFormat format, TextureType_NativeInterop type, uint mipLevels, uint* width, uint* height, uint* depth, byte** data);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Texture_Dispose(IntPtr handle);

		public Texture2D(Device device, TextureMode mode)
		{
			handle = Orbital_Video_D3D12_Texture_Create(device.handle, mode);
		}

		public unsafe bool Init(TextureFormat format, int width, int height, byte[] data)
		{
			fixed (byte* dataPtr = data)
			{
				uint widthValue = (uint)width;
				uint heightValue = (uint)height;
				uint depthValue = 1;
				return Orbital_Video_D3D12_Texture_Init(handle, format, TextureType_NativeInterop._2D, 1, &widthValue, &heightValue, &depthValue, &dataPtr) != 0;
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_Texture_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
