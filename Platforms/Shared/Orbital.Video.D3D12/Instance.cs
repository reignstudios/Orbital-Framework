using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public enum FeatureLevel
	{
		Level_11_0,
		Level_11_1,
		Level_12_0,
		Level_12_1
	}

	public struct InstanceDesc
	{
		public FeatureLevel minimumFeatureLevel;
		public bool extraDebugging;
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib = "Orbital.Video.D3D12.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		internal IntPtr handle;

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Instance_Create();

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern int Orbital_Video_D3D12_Instance_Init(IntPtr handle, FeatureLevel minimumFeatureLevel, int extraDebugging);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern void Orbital_Video_D3D12_Instance_Dispose(IntPtr handle);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_Instance_QuerySupportedAdapters(IntPtr handle, int allowSoftwareAdapters, AdapterInfo_NativeInterop* adapters, int* adapterCount, int adapterNameMaxLength);

		public Instance()
		{
			handle = Orbital_Video_D3D12_Instance_Create();
		}

		public bool Init(InstanceDesc desc)
		{
			return Orbital_Video_D3D12_Instance_Init(handle, desc.minimumFeatureLevel, desc.extraDebugging ? 1 : 0) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_Instance_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override unsafe bool QuerySupportedAdapters(bool allowSoftwareAdapters, out AdapterInfo[] adapters)
		{
			const int maxAdapters = 16, maxNameLength = 64;
			int adapterNameCount = maxAdapters;
			var adapters_Native = stackalloc AdapterInfo_NativeInterop[maxAdapters];
			for (int i = 0; i != maxAdapters; ++i)
			{
				char* adapterNamePtr = stackalloc char[maxNameLength];
				adapters_Native[i].name = (IntPtr)adapterNamePtr;
			}

			if (Orbital_Video_D3D12_Instance_QuerySupportedAdapters(handle, (byte)(allowSoftwareAdapters ? 1 : 0), adapters_Native, &adapterNameCount, maxNameLength) == 0)
			{
				adapters = null;
				return false;
			}

			adapters = new AdapterInfo[adapterNameCount];
			for (int i = 0; i != adapterNameCount; ++i)
			{
				var adapter = &adapters_Native[i];
				adapters[i] = new AdapterInfo
				(
					adapter->isPrimary != 0 ? true : false,
					adapter->index,
					Marshal.PtrToStringUni(adapter->name),
					adapter->vendorID,
					adapter->nodeCount,
					adapter->dedicatedGPUMemory.ToUInt64(),
					adapter->deticatedSystemMemory.ToUInt64(),
					adapter->sharedSystemMemory.ToUInt64()
				);
			}
			return true;
		}
	}
}
