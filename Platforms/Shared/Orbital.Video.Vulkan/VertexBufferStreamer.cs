using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class VertexBufferStreamer : VertexBufferStreamerBase
	{
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_VertexBufferStreamer_Create();

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_VertexBufferStreamer_Init(IntPtr handle, VertexBufferStreamLayout_NativeInterop* layout);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_VertexBufferStreamer_Dispose(IntPtr handle);

		public VertexBufferStreamer()
		{
			handle = Orbital_Video_D3D12_VertexBufferStreamer_Create();
		}

		public unsafe bool Init(VertexBufferStreamLayout layout)
		{
			using (var layoutNative = new VertexBufferStreamLayout_NativeInterop(ref layout))
			{
				return Orbital_Video_D3D12_VertexBufferStreamer_Init(handle, &layoutNative) != 0;
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_VertexBufferStreamer_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
