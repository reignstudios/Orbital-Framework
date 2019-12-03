using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class VertexBuffer : VertexBufferBase
	{
		private IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_VertexBuffer_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_VertexBuffer_Init(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_VertexBuffer_Dispose(IntPtr handle);

		public VertexBuffer(Device device)
		{
			handle = Orbital_Video_D3D12_VertexBuffer_Create(device.handle);
		}

		public bool Init()
		{
			return Orbital_Video_D3D12_VertexBuffer_Init(handle) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_VertexBuffer_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
