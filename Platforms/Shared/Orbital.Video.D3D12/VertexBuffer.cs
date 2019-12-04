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
		private static unsafe extern int Orbital_Video_D3D12_VertexBuffer_Init(IntPtr handle, void* vertices, ulong vertexCount, uint vertexSize);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_VertexBuffer_Dispose(IntPtr handle);

		public VertexBuffer(Device device)
		{
			handle = Orbital_Video_D3D12_VertexBuffer_Create(device.handle);
		}

		#if CS_7_3
		public unsafe bool Init<T>(T[] vertices) where T : unmanaged
		{
			fixed (T* verticesPtr = vertices)
			{
				return Orbital_Video_D3D12_VertexBuffer_Init(handle, verticesPtr, (ulong)vertices.LongLength, (uint)Marshal.SizeOf<T>()) != 0;
			}
		}
		#else
		public unsafe bool Init<T>(T[] vertices) where T : struct
		{
			byte[] verticesDataCopy = new byte[Marshal.SizeOf<T>() * vertices.Length];
			var gcHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
			Marshal.Copy(gcHandle.AddrOfPinnedObject(), verticesDataCopy, 0, verticesDataCopy.Length);
			gcHandle.Free();
			fixed (byte* verticesPtr = verticesDataCopy)
			{
				return Orbital_Video_D3D12_VertexBuffer_Init(handle, verticesPtr, (ulong)vertices.LongLength, (uint)Marshal.SizeOf<T>()) != 0;
			}
		}
		#endif

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
