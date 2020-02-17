using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class DepthStencil : DepthStencilBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_DepthStencil_Create(IntPtr device, DepthStencilMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_DepthStencil_Init(IntPtr handle, DepthStencilFormat format, uint width, uint height);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_DepthStencil_Dispose(IntPtr handle);

		public DepthStencil(Device device, DepthStencilMode mode)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_DepthStencil_Create(device.handle, mode);
		}

		public unsafe bool Init(DepthStencilFormat format, int width, int height)
		{
			this.width = width;
			this.height = height;
			return Orbital_Video_D3D12_DepthStencil_Init(handle, format, (uint)width, (uint)height) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_DepthStencil_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
