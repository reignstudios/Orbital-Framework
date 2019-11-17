using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.Vulkan
{
	public sealed class SwapChain : SwapChainBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;
		private readonly bool ensureSwapChainMatchesWindowSize;
		private bool sizeEnforced;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_SwapChain_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_Vulkan_SwapChain_Init(IntPtr handle, IntPtr hWnd, ref uint width, ref uint height, ref int sizeEnforced, uint bufferCount, int fullscreen);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_SwapChain_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_SwapChain_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_SwapChain_Present(IntPtr handle);

		public SwapChain(Device device, bool ensureSwapChainMatchesWindowSize)
		: base(device)
		{
			deviceVulkan = device;
			handle = Orbital_Video_Vulkan_SwapChain_Create(device.handle);
			this.ensureSwapChainMatchesWindowSize = ensureSwapChainMatchesWindowSize;
		}

		internal bool Init(WindowBase window, int bufferCount, bool fullscreen)
		{
			var size = window.GetSize(WindowSizeType.WorkingArea);
			uint width = (uint)size.width;
			uint height = (uint)size.height;
			int sizeEnforcedResult = 0;
			IntPtr hWnd = window.GetHandle();
			if (Orbital_Video_Vulkan_SwapChain_Init(handle, hWnd, ref width, ref height, ref sizeEnforcedResult, (uint)bufferCount, (fullscreen ? 1 : 0)) == 0) return false;
			sizeEnforced = sizeEnforcedResult != 0 || width != size.width || height != size.height;
			return true;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_SwapChain_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			if (ensureSwapChainMatchesWindowSize && !sizeEnforced)
			{
				// TODO: check if window size changed and resize swapchain back-buffer if so to match
			}
			Orbital_Video_Vulkan_SwapChain_BeginFrame(handle);
		}

		public override void Present()
		{
			Orbital_Video_Vulkan_SwapChain_Present(handle);
		}
	}
}
