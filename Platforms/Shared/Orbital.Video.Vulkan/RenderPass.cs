using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class RenderPass : RenderPassBase
	{
		internal IntPtr handle;
		private readonly SwapChain swapChain;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_RenderPass_Create_WithSwapChain(IntPtr device, IntPtr swapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_Vulkan_RenderPass_Init(IntPtr handle, RenderPassDesc_NativeInterop* desc);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_RenderPass_Dispose(IntPtr handle);

		public RenderPass(SwapChain swapChain)
		{
			this.swapChain = swapChain;
			handle = Orbital_Video_Vulkan_RenderPass_Create_WithSwapChain(swapChain.deviceVulkan.handle, swapChain.handle);
			this.swapChain.renderPasses.Add(this);
		}

		public unsafe bool Init(RenderPassDesc desc)
		{
			var descNative = new RenderPassDesc_NativeInterop(ref desc);
			return Orbital_Video_Vulkan_RenderPass_Init(handle, &descNative) != 0;
		}

		public override void Dispose()
		{
			swapChain.renderPasses.Remove(this);

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_RenderPass_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		internal void ResizeFrameBuffer()
		{
			// TODO: invoke native method to resize frameBuffer objects
		}
	}
}
