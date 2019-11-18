using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class RenderPass : RenderPassBase
	{
		internal IntPtr handle;
		private readonly SwapChain swapChainVulkan;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_RenderPass_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_Vulkan_RenderPass_Init_SwapChain(IntPtr handle, RenderPassDesc* desc, IntPtr swapChain);

		public RenderPass(SwapChain swapChain)
		{
			swapChainVulkan = swapChain;
			handle = Orbital_Video_Vulkan_RenderPass_Create(swapChain.deviceVulkan.handle);
		}

		public unsafe bool Init(RenderPassDesc desc)
		{
			return Orbital_Video_Vulkan_RenderPass_Init_SwapChain(handle, &desc, swapChainVulkan.handle) != 0;
		}
	}
}
