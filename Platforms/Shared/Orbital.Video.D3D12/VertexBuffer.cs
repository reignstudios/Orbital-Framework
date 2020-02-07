using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class VertexBuffer : VertexBufferBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;
		public IndexBuffer indexBufferD3D12 { get; private set; }
		private readonly VertexBufferMode mode;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_VertexBuffer_Create(IntPtr device, VertexBufferMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_VertexBuffer_Init(IntPtr handle, void* vertices, uint vertexCount, uint vertexSize);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_VertexBuffer_Dispose(IntPtr handle);

		public VertexBuffer(Device device, VertexBufferMode mode)
		: base(device)
		{
			deviceD3D12 = device;
			this.mode = mode;
			handle = Orbital_Video_D3D12_VertexBuffer_Create(device.handle, mode);
		}

		public unsafe bool Init(uint vertexCount, uint vertexSize)
		{
			return Orbital_Video_D3D12_VertexBuffer_Init(handle, null, vertexCount, vertexSize) != 0;
		}

		#if CS_7_3
		public unsafe bool Init<T>(T[] vertices) where T : unmanaged
		{
			vertexCount = vertices.Length;
			vertexSize = Marshal.SizeOf<T>();
			fixed (T* verticesPtr = vertices)
			{
				return Orbital_Video_D3D12_VertexBuffer_Init(handle, verticesPtr, (uint)vertices.LongLength, (uint)vertexSize) != 0;
			}
		}
		#else
		public unsafe bool Init<T>(T[] vertices) where T : struct
		{
			vertexCount = vertices.Length;
			vertexSize = Marshal.SizeOf<T>();
			byte[] verticesDataCopy = new byte[vertexSize * vertices.Length];
			var gcHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
			Marshal.Copy(gcHandle.AddrOfPinnedObject(), verticesDataCopy, 0, verticesDataCopy.Length);
			gcHandle.Free();
			fixed (byte* verticesPtr = verticesDataCopy)
			{
				return Orbital_Video_D3D12_VertexBuffer_Init(handle, verticesPtr, (uint)vertices.LongLength, (uint)vertexSize) != 0;
			}
		}
		#endif

		public bool Init<T>(T[] vertices, ushort[] indices)
		#if CS_7_3
		where T : unmanaged
		#else
		where T : struct
		#endif
		{
			indexBufferD3D12 = new IndexBuffer(deviceD3D12, GetIndexBufferMode(mode));
			if (!indexBufferD3D12.Init(indices)) return false;
			indexBuffer = indexBufferD3D12;
			return Init<T>(vertices);
		}

		public bool Init<T>(T[] vertices, uint[] indices)
		#if CS_7_3
		where T : unmanaged
		#else
		where T : struct
		#endif
		{
			indexBufferD3D12 = new IndexBuffer(deviceD3D12, GetIndexBufferMode(mode));
			if (!indexBufferD3D12.Init(indices)) return false;
			indexBuffer = indexBufferD3D12;
			return Init<T>(vertices);
		}

		public override void Dispose()
		{
			indexBuffer = null;
			if (indexBufferD3D12 != null)
			{
				indexBufferD3D12.Dispose();
				indexBufferD3D12 = null;
			}

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_VertexBuffer_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
