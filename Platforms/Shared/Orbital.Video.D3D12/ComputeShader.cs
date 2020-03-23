using System;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.IO;

namespace Orbital.Video.D3D12
{
	public sealed class ComputeShader : ComputeShaderBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_ComputeShader_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern unsafe int Orbital_Video_D3D12_ComputeShader_Init(IntPtr handle, byte* bytecode, uint bytecodeLength, ComputeShaderDesc_NativeInterop* desc);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_ComputeShader_Dispose(IntPtr handle);

		public ComputeShader(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_ComputeShader_Create(device.handle);
		}

		public bool Init(Stream stream, ComputeShaderDesc desc)
		{
			stream.ToByteArray(out byte[] bytecode);
			return Init(bytecode, 0, bytecode.Length, desc);
		}

		public bool Init(byte[] bytecode, ComputeShaderDesc desc)
		{
			return Init(bytecode, 0, bytecode.Length, desc);
		}

		public unsafe bool Init(byte[] bytecode, int offset, int length, ComputeShaderDesc desc)
		{
			using (var nativeDesc = new ComputeShaderDesc_NativeInterop(ref desc))
			{
				fixed (byte* bytecodePtr = bytecode) return Orbital_Video_D3D12_ComputeShader_Init(handle, bytecodePtr + offset, (uint)length, &nativeDesc) != 0;
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_ComputeShader_Dispose(handle);
			}
		}
	}
}
