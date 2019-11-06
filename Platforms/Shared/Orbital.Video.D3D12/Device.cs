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

	public sealed class Device : DeviceBase
	{
		private IntPtr handle;
		private const string lib = "Orbital.Video.D3D12.Native.dll";

		[DllImport(lib)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create();

		[DllImport(lib)]
		private static extern byte Orbital_Video_D3D12_Device_Init(IntPtr device, int adapterIndex, FeatureLevel minimumFeatureLevel, byte softwareRasterizer);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr device);

		public Device(DeviceType type)
		: base(type)
		{
			handle = Orbital_Video_D3D12_Device_Create();
		}

		public bool Init(int adapterIndex, FeatureLevel minimumFeatureLevel, bool softwareRasterizer)
		{
			return Orbital_Video_D3D12_Device_Init(handle, adapterIndex, minimumFeatureLevel, (byte)(softwareRasterizer ? 1 : 0)) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			throw new NotImplementedException();
		}

		public override void EndFrame()
		{
			throw new NotImplementedException();
		}
	}
}
