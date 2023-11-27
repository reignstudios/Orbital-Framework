using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Orbital.Host;
using Orbital.Numerics;

namespace Orbital.Video.Vulkan
{
	public sealed class SwapChain : SwapChainBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;
		private readonly bool ensureSizeMatchesWindowSize;
		private bool sizeEnforced;
		internal List<RenderPass> renderPasses = new List<RenderPass>();

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

		public SwapChain(Device device, bool ensureSizeMatchesWindowSize, SwapChainType type)
		: base(device, type)
		{
			deviceVulkan = device;
			handle = Orbital_Video_Vulkan_SwapChain_Create(device.handle);
			this.ensureSizeMatchesWindowSize = ensureSizeMatchesWindowSize;
		}

		public bool Init(WindowBase window, int bufferCount, bool fullscreen)
		{
			//InitBase(type);
			var size = window.GetSize();
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
			if (ensureSizeMatchesWindowSize && !sizeEnforced)
			{
				// TODO: check if window size changed and resize swapchain back-buffer if so to match
				foreach (var renderPass in renderPasses) renderPass.ResizeFrameBuffer();
			}
			Orbital_Video_Vulkan_SwapChain_BeginFrame(handle);
		}

		public override void Present()
		{
			Orbital_Video_Vulkan_SwapChain_Present(handle);
		}

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture)
		{
			throw new NotImplementedException();
		}

		public override void CopyTexture(Texture2DBase sourceTexture)
		{
			throw new NotImplementedException();
		}

		public override void CopyTexture(Texture2DBase sourceTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel)
		{
			throw new NotImplementedException();
		}

		#region Create Methods
		public override RenderPassBase CreateRenderPass(RenderPassDesc desc)
		{
			var abstraction = new RenderPass(this);
			if (!abstraction.Init(desc))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderPass");
			}
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase swapChain)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
