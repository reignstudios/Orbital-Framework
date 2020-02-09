using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class ConstantBuffer : ConstantBufferBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_ConstantBuffer_Create(IntPtr device, ConstantBufferMode mode);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_ConstantBuffer_Init(IntPtr handle, uint size, int* alignedSize, void* initialData);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_ConstantBuffer_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_ConstantBuffer_BeginUpdate(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern void Orbital_Video_D3D12_ConstantBuffer_EndUpdate(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern void Orbital_Video_D3D12_ConstantBuffer_Update(IntPtr handle, void* data, uint dataSize, uint dstOffset);

		public ConstantBuffer(Device device, ConstantBufferMode mode)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_ConstantBuffer_Create(device.handle, mode);
		}

		public unsafe bool Init(int size)
		{
			int alignedSize;
			int result = Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)size, &alignedSize, null);
			this.size = alignedSize;
			return result != 0;
		}

		public unsafe bool Init<T>() where T : struct
		{
			int alignedSize;
			int result = Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)Marshal.SizeOf<T>(), &alignedSize, null);
			size = alignedSize;
			return result != 0;
		}

		#if CS_7_3
		public unsafe bool Init<T>(T initialData) where T : unmanaged
		{
			int alignedSize;
			int result = Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)Marshal.SizeOf<T>(), &alignedSize, &initialData);
			size = alignedSize;
			return result != 0;
		}
		#else
		public unsafe bool Init<T>(T initialData) where T : struct
		{
			int alignedSize;
			TypedReference reference = __makeref(initialData);
			byte* ptr = (byte*)*((IntPtr*)&reference);
			#if MONO
			ptr += Marshal.SizeOf(typeof(RuntimeTypeHandle));
			#endif
			int result = Orbital_Video_D3D12_ConstantBuffer_Init(handle, (uint)Marshal.SizeOf<T>(), &alignedSize, ptr);
			size = alignedSize;
			return result != 0;
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

		public override bool BeginUpdate()
		{
			return Orbital_Video_D3D12_ConstantBuffer_BeginUpdate(handle) != 0;
		}

		public override void EndUpdate()
		{
			Orbital_Video_D3D12_ConstantBuffer_EndUpdate(handle);
		}

		#if CS_7_3
		public unsafe override void Update<T>(T data)
		{
			Orbital_Video_D3D12_ConstantBuffer_Update(handle, &data, (uint)Marshal.SizeOf<T>(), 0);
		}

		public unsafe override void Update<T>(T data, int offset)
		{
			Orbital_Video_D3D12_ConstantBuffer_Update(handle, &data, (uint)Marshal.SizeOf<T>(), (uint)offset);
		}
		#else
		public unsafe override void Update<T>(T data)
		{
			Update<T>(data, 0);
		}

		public unsafe override void Update<T>(T data, int offset)
		{
			TypedReference reference = __makeref(data);
			byte* ptr = (byte*)*((IntPtr*)&reference);
			#if MONO
			ptr += Marshal.SizeOf(typeof(RuntimeTypeHandle));
			#endif
			Orbital_Video_D3D12_ConstantBuffer_Update(handle, ptr, (uint)Marshal.SizeOf<T>(), (uint)offset);
		}
		#endif

		public override unsafe void Update(void* data, int dataSize, int offset)
		{
			Orbital_Video_D3D12_ConstantBuffer_Update(handle, data, (uint)dataSize, (uint)offset);
		}
	}
}
