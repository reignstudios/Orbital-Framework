using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public enum FeatureLevel
	{
		Level_1_0,
		Level_1_1,
		Level_1_2
	}

	public struct InstanceDesc
	{
		public FeatureLevel minimumFeatureLevel;
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib = "Orbital.Video.Vulkan.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		internal IntPtr handle;

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_Instance_Create();

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern int Orbital_Video_Vulkan_Instance_Init(IntPtr handle, FeatureLevel minimumFeatureLevel);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern void Orbital_Video_Vulkan_Instance_Dispose(IntPtr handle);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static unsafe extern int Orbital_Video_Vulkan_Instance_QuerySupportedAdapters(IntPtr handle, byte** adapterNames, uint adapterNameMaxLength, uint* adapterIndices, uint* adapterCount);

		public Instance()
		{
			handle = Orbital_Video_Vulkan_Instance_Create();
		}

		public bool Init(InstanceDesc desc)
		{
			return Orbital_Video_Vulkan_Instance_Init(handle, desc.minimumFeatureLevel) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_Instance_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override unsafe bool QuerySupportedAdapters(bool allowSoftwareAdapters, out AdapterInfo[] adapters)
		{
			/*const int maxNameLength = 64, maxAdapters = 32;
			uint adapterNameCount = maxAdapters;
			byte** adapterNamesPtr = stackalloc byte*[maxAdapters];
			for (int i = 0; i != maxAdapters; ++i)
			{
				byte* adapterNamePtr = stackalloc byte[maxNameLength];
				adapterNamesPtr[i] = adapterNamePtr;
			}
			uint* adapterIndices = stackalloc uint[maxAdapters];
			if (Orbital_Video_Vulkan_Instance_QuerySupportedAdapters(handle, adapterNamesPtr, maxNameLength, adapterIndices, &adapterNameCount) == 0)
			{
				adapters = null;
				return false;
			}

			adapters = new AdapterInfo[adapterNameCount];
			for (int i = 0; i != adapterNameCount; ++i)
			{
				string name = Marshal.PtrToStringAnsi((IntPtr)adapterNamesPtr[i]);
				adapters[i] = new AdapterInfo((int)adapterIndices[i], name);
			}
			return true;*/

			adapters = null;
			return false;// TODO: update
		}
	}
}
