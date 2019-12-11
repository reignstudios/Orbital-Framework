using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class ConstantBuffer : ConstantBufferBase
	{
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_ConstantBuffer_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_ConstantBuffer_Init(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_ConstantBuffer_Dispose(IntPtr handle);

		public ConstantBuffer(Device device)
		{
			handle = Orbital_Video_D3D12_ConstantBuffer_Create(device.handle);
		}

		public bool Init()
		{
			return Orbital_Video_D3D12_ConstantBuffer_Init(handle) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_ConstantBuffer_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
