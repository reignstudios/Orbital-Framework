using System;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Video.Vulkan
{
	static class CommandList
	{
		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern IntPtr Orbital_Video_Vulkan_CommandList_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern int Orbital_Video_Vulkan_CommandList_Init(IntPtr handle, CommandListType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_Start(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_Finish(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_BeginRenderPass(IntPtr handle, IntPtr renderPass);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_EndRenderPass(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_ClearSwapChainRenderTarget(IntPtr handle, IntPtr swapChain, float r, float g, float b, float a);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_Vulkan_CommandList_Execute(IntPtr handle);
	}
}
