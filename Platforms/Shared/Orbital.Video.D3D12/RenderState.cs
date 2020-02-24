using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class RenderState : RenderStateBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;
		internal int vertexCount, indexCount;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_RenderState_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_RenderState_Init(IntPtr handle, RenderStateDesc_NativeInterop* desc, uint gpuIndex);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_RenderState_Dispose(IntPtr handle);

		public RenderState(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_RenderState_Create(device.handle);
		}

		public unsafe bool Init(RenderStateDesc desc, int gpuIndex)
		{
			InitBase(ref desc);
			var streamer = (VertexBufferStreamer)desc.vertexBufferStreamer;
			vertexCount = streamer.vertexCount;
			if (desc.indexBuffer != null) indexCount = desc.indexBuffer.indexCount;
			using (var nativeDesc = new RenderStateDesc_NativeInterop(ref desc))
			{
				return Orbital_Video_D3D12_RenderState_Init(handle, &nativeDesc, (uint)gpuIndex) != 0;
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_RenderState_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
