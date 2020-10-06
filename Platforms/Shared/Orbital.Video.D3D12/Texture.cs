using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	static class Texture
	{
		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern IntPtr Orbital_Video_D3D12_Texture_Create(IntPtr device, TextureMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static unsafe extern int Orbital_Video_D3D12_Texture_Init(IntPtr handle, TextureFormat format, TextureType_NativeInterop type, uint mipLevels, uint* width, uint* height, uint* depth, byte** data, int isRenderTexture, int allowReadWrite, MSAALevel msaaLevel);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_Texture_Dispose(IntPtr handle);
	}
}
