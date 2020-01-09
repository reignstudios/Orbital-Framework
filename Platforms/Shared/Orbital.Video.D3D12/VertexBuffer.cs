using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class VertexBuffer : VertexBufferBase
	{
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_VertexBuffer_Create(IntPtr device, VertexBufferMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_VertexBuffer_Init(IntPtr handle, void* vertices, ulong vertexCount, uint vertexSize, VertexBufferLayout_NativeInterop* layout);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_VertexBuffer_Dispose(IntPtr handle);

		public VertexBuffer(Device device, VertexBufferMode mode)
		{
			handle = Orbital_Video_D3D12_VertexBuffer_Create(device.handle, mode);
		}

		public unsafe bool Init(long size, VertexBufferLayout layout)
		{
			var layoutNative = new VertexBufferLayout_NativeInterop(ref layout);
			return Orbital_Video_D3D12_VertexBuffer_Init(handle, null, (ulong)size, sizeof(byte), &layoutNative) != 0;
		}

		#if CS_7_3
		public unsafe bool Init<T>(T[] vertices, VertexBufferLayout layout) where T : unmanaged
		{
			var layoutNative = new VertexBufferLayout_NativeInterop(ref layout);
			vertexCount = vertices.Length;
			vertexSize = Marshal.SizeOf<T>();
			fixed (T* verticesPtr = vertices)
			{
				return Orbital_Video_D3D12_VertexBuffer_Init(handle, verticesPtr, (ulong)vertices.LongLength, (uint)vertexSize, &layoutNative) != 0;
			}
		}
		#else
		public unsafe bool Init<T>(T[] vertices, VertexBufferLayout layout) where T : struct
		{
			var layoutNative = new VertexBufferLayout_NativeInterop(ref layout);
			vertexCount = vertices.Length;
			vertexSize = Marshal.SizeOf<T>();
			byte[] verticesDataCopy = new byte[vertexSize * vertices.Length];
			var gcHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
			Marshal.Copy(gcHandle.AddrOfPinnedObject(), verticesDataCopy, 0, verticesDataCopy.Length);
			gcHandle.Free();
			fixed (byte* verticesPtr = verticesDataCopy)
			{
				return Orbital_Video_D3D12_VertexBuffer_Init(handle, verticesPtr, (ulong)vertices.LongLength, (uint)vertexSize, &layoutNative) != 0;
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
