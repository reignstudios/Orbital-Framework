using System;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.IO;

namespace Orbital.Video.Vulkan
{
	public sealed class ComputeShader : ComputeShaderBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;

		public ComputeShader(Device device)
		: base(device)
		{
			throw new NotImplementedException();
		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
