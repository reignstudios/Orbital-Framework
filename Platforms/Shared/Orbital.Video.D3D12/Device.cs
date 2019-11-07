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
		internal SwapChain swapChain;

		internal const string lib = "Orbital.Video.D3D12.Native.dll";

		[DllImport(lib)]
		private static unsafe extern byte Orbital_Video_D3D12_Device_QuerySupportedAdapters(FeatureLevel minimumFeatureLevel, byte allowSoftwareAdapters, char** adapterNames, uint* adapterNameCount, uint adapterNameMaxLength);

		[DllImport(lib)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create();

		[DllImport(lib)]
		private static extern byte Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, FeatureLevel minimumFeatureLevel, byte softwareRasterizer);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_BeginFrame(IntPtr handle);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_EndFrame(IntPtr handle);

		[DllImport(lib)]
		private static extern void Orbital_Video_D3D12_Device_ExecuteCommandBuffer(IntPtr handle, IntPtr commandBuffer);

		public static unsafe bool QuerySupportedAdapters(FeatureLevel minimumFeatureLevel, bool allowSoftwareAdapters, out string[] adapterNames)
		{
			const int maxNameLength = 128, maxNames = 32;
			uint adapterNameCount = maxNames;
			char** adapterNamesPtr = stackalloc char*[maxNames];
			for (int i = 0; i != maxNames; ++i)
			{
				char* adapterNamePtr = stackalloc char[maxNameLength];
				adapterNamesPtr[i] = adapterNamePtr;
			}

			if (Orbital_Video_D3D12_Device_QuerySupportedAdapters(minimumFeatureLevel, (byte)(allowSoftwareAdapters ? 1 : 0), adapterNamesPtr, &adapterNameCount, maxNameLength) == 0)
			{
				adapterNames = null;
				return false;
			}

			adapterNames = new string[adapterNameCount];
			for (int i = 0; i != adapterNameCount; ++i)
			{
				adapterNames[i] = Marshal.PtrToStringUni((IntPtr)adapterNamesPtr[i]);
			}
			return true;
		}

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
			swapChain.BeginFrame();
			// TODO: check if window size changed and resize swapchain back-buffer if so to match
			Orbital_Video_D3D12_Device_BeginFrame(handle);
		}

		public override void EndFrame()
		{
			if (type == DeviceType.Presentation)
			{
				swapChain.Present();
				Orbital_Video_D3D12_Device_EndFrame(handle);
			}
		}

		public override void ExecuteCommandBuffer(CommandBufferBase commandBuffer)
		{
			var commandBufferD3D12 = (CommandBuffer)commandBuffer;
			Orbital_Video_D3D12_Device_ExecuteCommandBuffer(handle, commandBufferD3D12.handle);
		}
	}
}
