using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class CommandList : CommandListBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_CommandList_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_Vulkan_CommandList_Init(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_Start(IntPtr handle, IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_Finish(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_BeginRenderPass(IntPtr handle, IntPtr renderPass);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_EndRenderPass(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(IntPtr handle, IntPtr swapChain, float r, float g, float b, float a);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_Execute(IntPtr handle);

		internal CommandList(Device device)
		: base(device)
		{
			deviceVulkan = device;
			handle = Orbital_Video_Vulkan_CommandList_Create(device.handle);
		}

		public bool Init()
		{
			return Orbital_Video_Vulkan_CommandList_Init(handle) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_CommandList_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Start()
		{
			Orbital_Video_Vulkan_CommandList_Start(handle, deviceVulkan.handle);
		}

		public override void Finish()
		{
			Orbital_Video_Vulkan_CommandList_Finish(handle);
		}

		public override void BeginRenderPass(RenderPassBase renderPass)
		{
			var renderPassVulkan = (RenderPass)renderPass;
			Orbital_Video_Vulkan_CommandList_BeginRenderPass(handle, renderPassVulkan.handle);
		}

		public override void EndRenderPass()
		{
			Orbital_Video_Vulkan_CommandList_EndRenderPass(handle);
		}

		public override void ClearRenderTarget(float r, float g, float b, float a)
		{
			Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(handle, deviceVulkan.swapChain.handle, r, b, g, a);
		}

		public override void ClearRenderTarget(SwapChainBase swapChain, float r, float g, float b, float a)
		{
			var swapChainVulkan = (SwapChain)swapChain;
			Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(handle, swapChainVulkan.handle, r, b, g, a);
		}

		public override void ClearRenderTarget(RenderTextureBase renderTarget, float r, float g, float b, float a)
		{
			throw new NotImplementedException();
		}

		public override void SetViewPort(ViewPort viewPort)
		{
			throw new NotImplementedException();
		}

		public override void SetRenderState(RenderStateBase renderState)
		{
			throw new NotImplementedException();
		}

		public override void Draw()
		{
			throw new NotImplementedException();
		}

		public override void Execute()
		{
			Orbital_Video_Vulkan_CommandList_Execute(handle);
		}
	}
}
