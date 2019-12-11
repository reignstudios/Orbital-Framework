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
		private static unsafe extern int Orbital_Video_D3D12_ConstantBuffer_Init(IntPtr handle, uint size, void* initialData);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_ConstantBuffer_Dispose(IntPtr handle);

		public ConstantBuffer(Device device)
		{
			handle = Orbital_Video_D3D12_ConstantBuffer_Create(device.handle);
		}

		public unsafe bool Init(int size)
		{
			this.size = size;
			return Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)size, null) != 0;
		}

		public unsafe bool Init<T>() where T : struct
		{
			size = Marshal.SizeOf<T>();
			return Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)size, null) != 0;
		}

		#if CS_7_3
		public unsafe bool Init<T>(T initialData) where T : unmanaged
		{
			size = Marshal.SizeOf<T>();
			return Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)size, &initialData) != 0;
		}
		#else
		public unsafe bool Init<T>(T initialData) where T : unmanaged
		{
			size = Marshal.SizeOf<T>();
			TypedReference reference = __makeref(initialData);
			byte* ptr = (byte*)*((IntPtr*)&reference);
			#if MONO
			ptr += Marshal.SizeOf(typeof(RuntimeTypeHandle));
			#endif
			return Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)size, ptr) != 0;
		}
		#endif

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
