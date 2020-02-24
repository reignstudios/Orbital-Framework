using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class RenderPass : RenderPassBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_RenderPass_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern int Orbital_Video_D3D12_RenderPass_Init_WithSwapChain(IntPtr handle, RenderPassDesc_NativeInterop* desc, IntPtr swapChain, IntPtr depthStencil);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern int Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(IntPtr handle, RenderPassDesc_NativeInterop* desc, IntPtr* renderTextures, RenderTextureUsage* usages, uint renderTextureCount, IntPtr depthStencil);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_RenderPass_Dispose(IntPtr handle);

		public RenderPass(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_RenderPass_Create(deviceD3D12.handle);
		}

		public unsafe bool Init(RenderPassDesc desc, SwapChain swapChain)
		{
			Validate(ref desc, 1);
			using (var descNative = new RenderPassDesc_NativeInterop(ref desc))
			{
				return Orbital_Video_D3D12_RenderPass_Init_WithSwapChain(handle, &descNative, swapChain.handle, IntPtr.Zero) != 0;
			}
		}

		public unsafe bool Init(RenderPassDesc desc, SwapChain swapChain, DepthStencil depthStencil)
		{
			Validate(ref desc, 1);
			using (var descNative = new RenderPassDesc_NativeInterop(ref desc))
			{
				return Orbital_Video_D3D12_RenderPass_Init_WithSwapChain(handle, &descNative, swapChain.handle, depthStencil.handle) != 0;
			}
		}

		public unsafe bool Init(RenderPassDesc desc, RenderTexture2D renderTexture)
		{
			Validate(ref desc, 1);
			using (var descNative = new RenderPassDesc_NativeInterop(ref desc))
			{
				var renderTextureHandle = renderTexture.handle;
				var usage = renderTexture.usage;
				return Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(handle, &descNative, &renderTextureHandle, &usage, 1, IntPtr.Zero) != 0;
			}
		}

		public unsafe bool Init(RenderPassDesc desc, RenderTexture2D renderTexture, DepthStencil depthStencil)
		{
			Validate(ref desc, 1);
			using (var descNative = new RenderPassDesc_NativeInterop(ref desc))
			{
				var renderTextureHandle = renderTexture.handle;
				var usage = renderTexture.usage;
				return Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(handle, &descNative, &renderTextureHandle, &usage, 1, depthStencil.handle) != 0;
			}
		}

		public unsafe bool Init(RenderPassDesc desc, RenderTexture2D[] renderTextures)
		{
			int length = renderTextures.Length;
			Validate(ref desc, length);
			using (var descNative = new RenderPassDesc_NativeInterop(ref desc))
			{
				var renderTextureHandles = stackalloc IntPtr[length];
				var usages = stackalloc RenderTextureUsage[length];
				for (int i = 0; i != length; ++i)
				{
					renderTextureHandles[i] = renderTextures[i].handle;
					usages[i] = renderTextures[i].usage;
				}
				return Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(handle, &descNative, renderTextureHandles, usages, (uint)length, IntPtr.Zero) != 0;
			}
		}

		public unsafe bool Init(RenderPassDesc desc, RenderTexture2D[] renderTextures, DepthStencil depthStencil)
		{
			int length = renderTextures.Length;
			Validate(ref desc, length);
			using (var descNative = new RenderPassDesc_NativeInterop(ref desc))
			{
				var renderTextureHandles = stackalloc IntPtr[length];
				var usages = stackalloc RenderTextureUsage[length];
				for (int i = 0; i != length; ++i)
				{
					renderTextureHandles[i] = renderTextures[i].handle;
					usages[i] = renderTextures[i].usage;
				}
				return Orbital_Video_D3D12_RenderPass_Init_WithRenderTextures(handle, &descNative, renderTextureHandles, usages, (uint)length, depthStencil.handle) != 0;
			}
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
