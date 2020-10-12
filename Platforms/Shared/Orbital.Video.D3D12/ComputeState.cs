using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class ComputeState : ComputeStateBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_ComputeState_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_ComputeState_Init(IntPtr handle, ComputeStateDesc_NativeInterop* desc);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_ComputeState_Dispose(IntPtr handle);

		public ComputeState(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_ComputeState_Create(device.handle);
		}

		public unsafe bool Init(ComputeStateDesc desc)
		{
			InitBase(ref desc);
			using (var nativeDesc = new ComputeStateDesc_NativeInterop(ref desc))
			{
				return Orbital_Video_D3D12_ComputeState_Init(handle, &nativeDesc) != 0;
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_ComputeState_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
