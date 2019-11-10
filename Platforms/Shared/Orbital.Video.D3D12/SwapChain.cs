using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.D3D12
{
	public sealed class SwapChain : SwapChainBase
	{
		internal IntPtr handle;
		public readonly Device deviceD3D12;

		[DllImport(Device.lib, CallingConvention = Device.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_SwapChain_Create();

		[DllImport(Device.lib, CallingConvention = Device.callingConvention)]
		private static extern int Orbital_Video_D3D12_SwapChain_Init(IntPtr handle, IntPtr device, IntPtr hWnd, uint width, uint height, uint bufferCount, int fullscreen);

		[DllImport(Device.lib, CallingConvention = Device.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_Dispose(IntPtr handle);

		[DllImport(Device.lib, CallingConvention = Device.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_BeginFrame(IntPtr handle);

		[DllImport(Device.lib, CallingConvention = Device.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_Present(IntPtr handle);

		public SwapChain(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_SwapChain_Create();
		}

		public bool Init(WindowBase window, int bufferCount, bool fullscreen)
		{
			var size = window.GetSize(WindowSizeType.WorkingArea);
			IntPtr hWnd = window.GetHandle();
			if (Orbital_Video_D3D12_SwapChain_Init(handle, deviceD3D12.handle, hWnd, (uint)size.width, (uint)size.height, (uint)bufferCount, (fullscreen ? 1 : 0)) == 0) return false;
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
