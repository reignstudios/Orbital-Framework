﻿using System;
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
		private static extern void Orbital_Video_Vulkan_CommandList_EnableSwapChainRenderTarget(IntPtr handle, IntPtr swapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_EnableSwapChainPresent(IntPtr handle, IntPtr swapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(IntPtr handle, IntPtr swapChain, float r, float g, float b, float a);

		internal CommandList(Device device)
		: base(device)
		{
			deviceVulkan = device;
			handle = Orbital_Video_Vulkan_CommandList_Create(device.handle);
		}

		internal bool Init()
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

		public override void EnabledRenderTarget()
		{
			Orbital_Video_Vulkan_CommandList_EnableSwapChainRenderTarget(handle, deviceVulkan.swapChain.handle);
		}

		public override void EnabledRenderTarget(DepthStencilBase depthStencil)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledRenderTarget(SwapChainBase swapChain)
		{
			var swapChainVulkan = (SwapChain)swapChain;
			Orbital_Video_Vulkan_CommandList_EnableSwapChainRenderTarget(handle, swapChainVulkan.handle);
		}

		public override void EnabledRenderTarget(SwapChainBase swapChain, DepthStencilBase depthStencil)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledRenderTarget(RenderTargetBase renderTarget)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledRenderTarget(RenderTargetBase renderTarget, DepthStencilBase depthStencil)
		{
			throw new System.NotImplementedException();
		}

		public override void EnabledPresent()
		{
			Orbital_Video_Vulkan_CommandList_EnableSwapChainPresent(handle, deviceVulkan.swapChain.handle);
		}

		public override void EnabledPresent(SwapChainBase swapChain)
		{
			var swapChainVulkan = (SwapChain)swapChain;
			Orbital_Video_Vulkan_CommandList_EnableSwapChainPresent(handle, swapChainVulkan.handle);
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

		public override void ClearRenderTarget(RenderTargetBase renderTarget, float r, float g, float b, float a)
		{
			throw new NotImplementedException();
		}
	}
}
