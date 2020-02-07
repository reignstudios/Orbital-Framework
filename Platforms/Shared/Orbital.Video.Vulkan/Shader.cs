using System;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.IO;

namespace Orbital.Video.Vulkan
{
	public sealed class Shader : ShaderBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;
		private readonly ShaderType type;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_Shader_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern unsafe int Orbital_Video_Vulkan_Shader_Init(IntPtr handle, byte* bytecode, uint bytecodeLength);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Shader_Dispose(IntPtr handle);

		public Shader(Device device, ShaderType type)
		: base(device)
		{
			deviceVulkan = device;
			this.type = type;
			handle = Orbital_Video_Vulkan_Shader_Create(device.handle);
		}

		public bool Init(Stream stream)
		{
			stream.ToByteArray(out byte[] bytecode);
			return Init(bytecode);
		}

		public bool Init(byte[] bytecode)
		{
			return Init(bytecode, 0, bytecode.Length);
		}

		public unsafe bool Init(byte[] bytecode, int offset, int length)
		{
			fixed (byte* bytecodePtr = bytecode) return Orbital_Video_Vulkan_Shader_Init(handle, bytecodePtr + offset, (uint)length) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_Shader_Dispose(handle);
			}
		}

		public override ShaderType GetShaderType()
		{
			return type;
		}
	}
}
