using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.Vulkan
{
	public struct DeviceDesc
	{
		/// <summary>
		/// Represents physical device index
		/// </summary>
		public int adapterIndex;

		/// <summary>
		/// Window to the device will present to. Can be null for background devices
		/// </summary>
		public WindowBase window;

		/// <summary>
		/// If the window size changes, auto resize the swap-chain to match if possible
		/// </summary>
		public bool ensureSwapChainMatchesWindowSizeIfPossible;

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
		public readonly Instance instanceVulkan;
		internal IntPtr handle;
		internal SwapChain swapChain;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_Device_Create(IntPtr Instance, DeviceType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_Vulkan_Device_Init(IntPtr handle, int adapterIndex);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_EndFrame(IntPtr handle);

		//[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		//private static extern void Orbital_Video_Vulkan_Device_ExecuteCommandList(IntPtr handle, IntPtr commandList);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceVulkan = instance;
			handle = Orbital_Video_Vulkan_Device_Create(instance.handle, type);
		}

		public bool Init(DeviceDesc desc)
		{
			if (Orbital_Video_Vulkan_Device_Init(handle, desc.adapterIndex) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChain = new SwapChain(this, desc.ensureSwapChainMatchesWindowSizeIfPossible);
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
				Orbital_Video_Vulkan_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			
		}

		public override void EndFrame()
		{
			
		}

		public override void ExecuteCommandList(CommandListBase commandList)
		{
			
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSwapChainMatchesWindowSize)
		{
			return null;
		}

		public override CommandListBase CreateCommandList()
		{
			return null;
		}
		#endregion
	}
}
