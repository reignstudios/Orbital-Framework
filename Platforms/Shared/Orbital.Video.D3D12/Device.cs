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
		internal IntPtr handle;
		private SwapChain swapChain;

		internal const string lib = "Orbital.Video.D3D12.Native.dll";

		[DllImport(lib)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create();

		[DllImport(lib)]
		private static extern byte Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, FeatureLevel minimumFeatureLevel, byte softwareRasterizer);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_WaitForFrameCompletion(IntPtr handle);

		public Device(DeviceType type)
		: base(type)
		{
			handle = Orbital_Video_D3D12_Device_Create();
			if (type == DeviceType.Presentation) swapChain = new SwapChain();
		}

		public bool Init(int adapterIndex, FeatureLevel minimumFeatureLevel, bool softwareRasterizer, IntPtr hWnd, int width, int height, int bufferCount, bool fullscreen)
		{
			if (Orbital_Video_D3D12_Device_Init(handle, adapterIndex, minimumFeatureLevel, (byte)(softwareRasterizer ? 1 : 0)) == 0) return false;
			return swapChain.Init(this, hWnd, width, height, bufferCount, fullscreen);
		}

		public override void Dispose()
		{
			if (swapChain != null)
			{
				swapChain.Dispose();
				swapChain = null;
			}

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			// TODO: check if window size changed and resize swapchain back-buffer if so to match
		}

		public override void EndFrame()
		{
			if (type == DeviceType.Presentation)
			{
				swapChain.Present();
				Orbital_Video_D3D12_Device_WaitForFrameCompletion(handle);
			}
		}
	}
}
