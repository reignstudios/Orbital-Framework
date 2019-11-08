using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.D3D12
{
	public enum FeatureLevel
	{
		Level_11_0,
		Level_11_1,
		Level_12_0,
		Level_12_1
	}

	public struct DeviceDesc
	{
		public int adapterIndex;
		public bool softwareRasterizer;
		public FeatureLevel minimumFeatureLevel;
		public WindowBase window;
		public bool ensureSwapChainMatchesWindowSize;
		public int swapChainBufferCount;
		public bool fullscreen;
	}

	public sealed class Device : DeviceBase
	{
		internal IntPtr handle;
		internal SwapChain swapChain;
		private WindowBase window;
		private bool ensureSwapChainMatchesWindowSize;

		internal const string lib = "Orbital.Video.D3D12.Native.dll";

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
		private static unsafe extern int Orbital_Video_D3D12_Device_QuerySupportedAdapters(FeatureLevel minimumFeatureLevel, int allowSoftwareAdapters, char** adapterNames, uint* adapterNameCount, uint adapterNameMaxLength);

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create();

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern int Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, FeatureLevel minimumFeatureLevel, int softwareRasterizer);

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Orbital_Video_D3D12_Device_BeginFrame(IntPtr handle);

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Orbital_Video_D3D12_Device_EndFrame(IntPtr handle);

		[DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
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
		}

		public bool Init(DeviceDesc desc)
		{
			window = desc.window;
			ensureSwapChainMatchesWindowSize = desc.ensureSwapChainMatchesWindowSize;

			if (Orbital_Video_D3D12_Device_Init(handle, desc.adapterIndex, desc.minimumFeatureLevel, (desc.softwareRasterizer ? 1 : 0)) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChain = new SwapChain(this);
				return swapChain.Init(desc.window, desc.swapChainBufferCount, desc.fullscreen);
			}
			else
			{
				return true;
			}
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
			if (ensureSwapChainMatchesWindowSize)
			{
				// TODO: check if window size changed and resize swapchain back-buffer if so to match
			}
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

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen)
		{
			var abstraction = new SwapChain(this);
			if (!abstraction.Init(window, bufferCount, fullscreen))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
		}

		public override CommandBufferBase CreateCommandBuffer()
		{
			var abstraction = new CommandBuffer(this);
			if (!abstraction.Init())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create CommandBuffer");
			}
			return abstraction;
		}
		#endregion
	}
}
