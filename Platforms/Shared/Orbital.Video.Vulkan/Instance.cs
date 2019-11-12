using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public enum FeatureLevel
	{
		Level_1_0,
		Level_1_1
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib = "Orbital.Video.Vulkan.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		internal IntPtr handle;

		public override void Dispose()
		{
			throw new NotImplementedException();
		}

		public override unsafe bool QuerySupportedAdapters(bool allowSoftwareAdapters, out AdapterInfo[] adapters)
		{
			throw new NotImplementedException();
		}
	}
}
