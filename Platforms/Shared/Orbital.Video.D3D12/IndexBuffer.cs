using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class IndexBuffer : IndexBufferBase
	{
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_IndexBuffer_Create(IntPtr device, IndexBufferMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_IndexBuffer_Init(IntPtr handle, void* indices, uint indexCount, uint indexSize);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_IndexBuffer_Dispose(IntPtr handle);

		public IndexBuffer(Device device, IndexBufferMode mode)
		{
			handle = Orbital_Video_D3D12_IndexBuffer_Create(device.handle, mode);
		}

		public unsafe bool Init(uint indexCount, IndexBufferSize indexSize)
		{
			
			this.indexSize = indexSize;
			return Orbital_Video_D3D12_IndexBuffer_Init(handle, null, indexCount, (uint)indexSize) != 0;
		}

		public unsafe bool Init(ushort[] indices)
		{
			indexCount = indices.Length;
			indexSize = IndexBufferSize.Bit_16;
			fixed (ushort* indicesPtr = indices)
			{
				return Orbital_Video_D3D12_IndexBuffer_Init(handle, indicesPtr, (uint)indices.LongLength, (uint)indexSize) != 0;
			}
		}

		public unsafe bool Init(uint[] indices)
		{
			indexCount = indices.Length;
			indexSize = IndexBufferSize.Bit_32;
			fixed (uint* indicesPtr = indices)
			{
				return Orbital_Video_D3D12_IndexBuffer_Init(handle, indicesPtr, (uint)indices.LongLength, (uint)indexSize) != 0;
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_IndexBuffer_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
