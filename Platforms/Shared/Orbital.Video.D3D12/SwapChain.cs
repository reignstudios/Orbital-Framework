using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class SwapChain : SwapChainBase
	{
		internal IntPtr handle;

		[DllImport(Device.lib)]
		private static extern IntPtr Orbital_Video_D3D12_SwapChain_Create();

		[DllImport(Device.lib)]
		private static extern byte Orbital_Video_D3D12_SwapChain_Init(IntPtr handle, IntPtr device, IntPtr hWnd, uint width, uint height, uint bufferCount, byte fullscreen);

		[DllImport(Device.lib)]
		private static extern void Orbital_Video_D3D12_SwapChain_Dispose(IntPtr handle);

		[DllImport(Device.lib)]
		private static extern void Orbital_Video_D3D12_SwapChain_BeginFrame(IntPtr handle);

		[DllImport(Device.lib)]
		private static extern void Orbital_Video_D3D12_SwapChain_Present(IntPtr handle);

		public SwapChain()
		{
			handle = Orbital_Video_D3D12_SwapChain_Create();
		}

		public bool Init(Device device, IntPtr hWnd, int width, int height, int bufferCount, bool fullscreen)
		{
			if (Orbital_Video_D3D12_SwapChain_Init(handle, device.handle, hWnd, (uint)width, (uint)height, (uint)bufferCount, (byte)(fullscreen ? 1 : 0)) == 0) return false;
			return true;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_SwapChain_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			Orbital_Video_D3D12_SwapChain_BeginFrame(handle);
		}

		public override void Present()
		{
			Orbital_Video_D3D12_SwapChain_Present(handle);
		}
	}
}
