using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class RenderPass : RenderPassBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(IntPtr device, IntPtr swapChain, IntPtr depthStencil);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern IntPtr Orbital_Video_D3D12_RenderPass_Create_WithRenderTextures(IntPtr device, IntPtr* renderTextures, uint renderTextureCount, IntPtr depthStencil);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_RenderPass_Init(IntPtr handle, RenderPassDesc_NativeInterop* desc);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_RenderPass_Dispose(IntPtr handle);

		public RenderPass(SwapChain swapChain)
		: base(swapChain.device)
		{
			deviceD3D12 = swapChain.deviceD3D12;
			handle = Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(deviceD3D12.handle, swapChain.handle, IntPtr.Zero);
		}

		public RenderPass(SwapChain swapChain, DepthStencil depthStencil)
		: base(swapChain.device)
		{
			deviceD3D12 = swapChain.deviceD3D12;
			handle = Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(deviceD3D12.handle, swapChain.handle, depthStencil.handle);
		}

		public unsafe RenderPass(RenderTexture2D renderTexture)
		: base(renderTexture.device)
		{
			deviceD3D12 = renderTexture.deviceD3D12;
			var renderTextureHandle = renderTexture.handle;
			handle = Orbital_Video_D3D12_RenderPass_Create_WithRenderTextures(deviceD3D12.handle, &renderTextureHandle, 1, IntPtr.Zero);
		}

		public unsafe RenderPass(RenderTexture2D renderTexture, DepthStencil depthStencil)
		: base(renderTexture.device)
		{
			deviceD3D12 = renderTexture.deviceD3D12;
			var renderTextureHandle = renderTexture.handle;
			handle = Orbital_Video_D3D12_RenderPass_Create_WithRenderTextures(deviceD3D12.handle, &renderTextureHandle, 1, depthStencil.handle);
		}

		public unsafe RenderPass(RenderTexture2D[] renderTextures)
		: base(renderTextures[0].device)
		{
			deviceD3D12 = renderTextures[0].deviceD3D12;
			int length = renderTextures.Length;
			var renderTextureHandles = stackalloc IntPtr[length];
			for (int i = 0; i != length; ++i) renderTextureHandles[i] = renderTextures[i].handle;
			handle = Orbital_Video_D3D12_RenderPass_Create_WithRenderTextures(deviceD3D12.handle, renderTextureHandles, (uint)length, IntPtr.Zero);
		}

		public unsafe RenderPass(RenderTexture2D[] renderTextures, DepthStencil depthStencil)
		: base(renderTextures[0].device)
		{
			deviceD3D12 = renderTextures[0].deviceD3D12;
			int length = renderTextures.Length;
			var renderTextureHandles = stackalloc IntPtr[length];
			for (int i = 0; i != length; ++i) renderTextureHandles[i] = renderTextures[i].handle;
			handle = Orbital_Video_D3D12_RenderPass_Create_WithRenderTextures(deviceD3D12.handle, renderTextureHandles, (uint)length, depthStencil.handle);
		}

		public unsafe bool Init(RenderPassDesc desc)
		{
			var descNative = new RenderPassDesc_NativeInterop(ref desc);
			return Orbital_Video_D3D12_RenderPass_Init(handle, &descNative) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_RenderPass_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}
	}
}
