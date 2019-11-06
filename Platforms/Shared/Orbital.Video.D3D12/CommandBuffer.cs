using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class CommandBuffer : CommandBufferBase
	{
		private IntPtr handle;

		[DllImport(Device.lib)]
		private static extern IntPtr Orbital_Video_D3D12_CommandBuffer_Create();

		[DllImport(Device.lib)]
		private static extern byte Orbital_Video_D3D12_CommandBuffer_Init(IntPtr handle);

		[DllImport(Device.lib)]
		private static extern void Orbital_Video_D3D12_CommandBuffer_Dispose(IntPtr handle);

		public CommandBuffer()
		{
			handle = Orbital_Video_D3D12_CommandBuffer_Create();
		}

		public bool Init()
		{
			return Orbital_Video_D3D12_CommandBuffer_Init(handle) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_CommandBuffer_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void EnabledRenderTarget(RenderTargetBase renderTarget)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledRenderTarget(RenderTargetBase renderTarget, DepthStencilBase depthStencil)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledRenderTarget(DeviceBase device)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledRenderTarget(DeviceBase device, DepthStencilBase depthStencil)
		{
			throw new System.NotImplementedException();
		}
	}
}
