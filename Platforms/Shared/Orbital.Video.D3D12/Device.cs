using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.D3D12
{
	public struct DeviceDesc
	{
		public int adapterIndex;
		public bool softwareRasterizer;
		public WindowBase window;
		public bool ensureSwapChainMatchesWindowSize;
		public int swapChainBufferCount;
		public bool fullscreen;
	}

	public sealed class Device : DeviceBase
	{
		public readonly Instance instanceD3D12;
		internal IntPtr handle;
		internal SwapChain swapChain;
		private WindowBase window;
		private bool ensureSwapChainMatchesWindowSize;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create(IntPtr Instance);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, FeatureLevel minimumFeatureLevel, int softwareRasterizer);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_EndFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_ExecuteCommandBuffer(IntPtr handle, IntPtr commandBuffer);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceD3D12 = instance;
			handle = Orbital_Video_D3D12_Device_Create(instance.handle);
		}

		public bool Init(DeviceDesc desc)
		{
			window = desc.window;
			ensureSwapChainMatchesWindowSize = desc.ensureSwapChainMatchesWindowSize;

			if (Orbital_Video_D3D12_Device_Init(handle, desc.adapterIndex, instanceD3D12.minimumFeatureLevel, (desc.softwareRasterizer ? 1 : 0)) == 0) return false;
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

		#region Abstraction Methods
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
