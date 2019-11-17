using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.D3D12
{
	public struct DeviceDesc
	{
		/// <summary>
		/// Represents physical device index
		/// </summary>
		public int adapterIndex;

		/// <summary>
		/// True if you want to create a WARP device
		/// </summary>
		public bool softwareRasterizer;

		/// <summary>
		/// Window to the device will present to. Can be null for background devices
		/// </summary>
		public WindowBase window;

		/// <summary>
		/// If the window size changes, auto resize the swap-chain to match
		/// </summary>
		public bool ensureSwapChainMatchesWindowSize;

		/// <summary>
		/// Double/Tripple buffering etc
		/// </summary>
		public int swapChainBufferCount;

		/// <summary>
		/// True to launch in fullscreen
		/// </summary>
		public bool fullscreen;
	}

	public sealed class Device : DeviceBase
	{
		public readonly Instance instanceD3D12;
		internal IntPtr handle;
		internal SwapChain swapChain;
		private WindowBase window;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_Device_Create(IntPtr Instance);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_Device_Init(IntPtr handle, int adapterIndex, int softwareRasterizer);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_EndFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_Device_ExecuteCommandList(IntPtr handle, IntPtr commandList);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceD3D12 = instance;
			handle = Orbital_Video_D3D12_Device_Create(instance.handle);
		}

		public bool Init(DeviceDesc desc)
		{
			window = desc.window;
			if (Orbital_Video_D3D12_Device_Init(handle, desc.adapterIndex, (desc.softwareRasterizer ? 1 : 0)) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChain = new SwapChain(this, desc.ensureSwapChainMatchesWindowSize);
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
			if (type == DeviceType.Presentation) swapChain.BeginFrame();
			Orbital_Video_D3D12_Device_BeginFrame(handle);
		}

		public override void EndFrame()
		{
			Orbital_Video_D3D12_Device_EndFrame(handle);
			if (type == DeviceType.Presentation) swapChain.Present();
		}

		public override void ExecuteCommandList(CommandListBase commandList)
		{
			var commandListD3D12 = (CommandList)commandList;
			Orbital_Video_D3D12_Device_ExecuteCommandList(handle, commandListD3D12.handle);
		}

		#region Abstraction Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSwapChainMatchesWindowSize)
		{
			var abstraction = new SwapChain(this, ensureSwapChainMatchesWindowSize);
			if (!abstraction.Init(window, bufferCount, fullscreen))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
		}

		public override CommandListBase CreateCommandList()
		{
			var abstraction = new CommandList(this);
			if (!abstraction.Init())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create CommandList");
			}
			return abstraction;
		}
		#endregion
	}
}
