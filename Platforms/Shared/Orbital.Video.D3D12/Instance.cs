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
	}

	public sealed class Instance : InstanceBase
	{
		public const string lib = "Orbital.Video.D3D12.Native.dll";
		public const CallingConvention callingConvention = CallingConvention.Cdecl;

		internal IntPtr handle;

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Instance_Create();

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern int Orbital_Video_D3D12_Instance_Init(IntPtr handle, FeatureLevel minimumFeatureLevel);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static extern void Orbital_Video_D3D12_Instance_Dispose(IntPtr handle);

		[DllImport(lib, CallingConvention = callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_Instance_QuerySupportedAdapters(IntPtr handle, int allowSoftwareAdapters, char** adapterNames, uint adapterNameMaxLength, uint* adapterIndices, uint* adapterCount);

		public Instance()
		{
			handle = Orbital_Video_D3D12_Instance_Create();
		}

		public bool Init(InstanceDesc desc)
		{
			return Orbital_Video_D3D12_Instance_Init(handle, desc.minimumFeatureLevel) != 0;
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
			const int maxNameLength = 128, maxAdapters = 32;
			uint adapterNameCount = maxAdapters;
			char** adapterNamesPtr = stackalloc char*[maxAdapters];
			for (int i = 0; i != maxAdapters; ++i)
			{
				char* adapterNamePtr = stackalloc char[maxNameLength];
				adapterNamesPtr[i] = adapterNamePtr;
			}
			uint* adapterIndices = stackalloc uint[maxAdapters];
			if (Orbital_Video_D3D12_Instance_QuerySupportedAdapters(handle, (byte)(allowSoftwareAdapters ? 1 : 0), adapterNamesPtr, maxNameLength, adapterIndices, &adapterNameCount) == 0)
			{
				adapters = null;
				return false;
			}

			adapters = new AdapterInfo[adapterNameCount];
			for (int i = 0; i != adapterNameCount; ++i)
			{
				string name = Marshal.PtrToStringUni((IntPtr)adapterNamesPtr[i]);
				adapters[i] = new AdapterInfo((int)adapterIndices[i], name);
			}
			return true;
		}
	}
}
