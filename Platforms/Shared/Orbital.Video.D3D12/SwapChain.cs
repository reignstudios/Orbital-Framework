using System;
using System.Runtime.InteropServices;
using Orbital.Host;
using Orbital.Numerics;

namespace Orbital.Video.D3D12
{
	public sealed class SwapChain : SwapChainBase
	{
		public readonly Device deviceD3D12;
		public DepthStencil depthStencilD3D12 { get; private set; }
		internal IntPtr handle;
		private readonly bool ensureSizeMatchesWindowSize;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_SwapChain_Create(IntPtr device, SwapChainType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_SwapChain_Init(IntPtr handle, IntPtr hWnd, uint width, uint height, uint bufferCount, int fullscreen, SwapChainFormat format);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern void Orbital_Video_D3D12_SwapChain_BeginFrame(IntPtr handle, int* currentNodeIndex, int* lastNodeIndex);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_Present(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_ResolveRenderTexture(IntPtr handle, IntPtr srcRenderTexture);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_CopyTexture(IntPtr handle, IntPtr srcTexture);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_SwapChain_CopyTextureRegion(IntPtr handle, IntPtr srcTexture, int srcX, int srcY, int dstX, int dstY, int width, int height, int srcMipmapLevel);

		public SwapChain(Device device, bool ensureSizeMatchesWindowSize, SwapChainType type)
		: base(device, type)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_SwapChain_Create(device.handle, this.type);
			this.ensureSizeMatchesWindowSize = ensureSizeMatchesWindowSize;
		}

		public bool Init(WindowBase window, int bufferCount, bool fullscreen, SwapChainFormat format)
		{
			var size = window.GetSize(WindowSizeType.WorkingArea);
			IntPtr hWnd = window.GetHandle();
			if (Orbital_Video_D3D12_SwapChain_Init(handle, hWnd, (uint)size.width, (uint)size.height, (uint)bufferCount, (fullscreen ? 1 : 0), format) == 0) return false;
			return true;
		}

		public bool Init(WindowBase window, int bufferCount, bool fullscreen, SwapChainFormat format, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode)
		{
			var size = window.GetSize(WindowSizeType.WorkingArea);
			depthStencilD3D12 = new DepthStencil(deviceD3D12, stencilUsage, depthStencilMode);
			depthStencil = depthStencilD3D12;
			if (!depthStencilD3D12.Init(size.width, size.height, depthStencilFormat, MSAALevel.Disabled)) return false;
			return Init(window, bufferCount, fullscreen, format);
		}

		public override void Dispose()
		{
			depthStencil = null;
			if (depthStencilD3D12 != null)
			{
				depthStencilD3D12.Dispose();
				depthStencilD3D12 = null;
			}

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_SwapChain_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public unsafe override void BeginFrame()
		{
			if (ensureSizeMatchesWindowSize)
			{
				// TODO: check if window size changed and resize swapchain back-buffer if so to match
			}
			int currentNodeIndex, lastNodeIndex;
			Orbital_Video_D3D12_SwapChain_BeginFrame(handle, &currentNodeIndex, &lastNodeIndex);
			this.currentNodeIndex = currentNodeIndex;
			this.lastNodeIndex = lastNodeIndex;
		}

		public override void Present()
		{
			Orbital_Video_D3D12_SwapChain_Present(handle);
		}

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture)
		{
			Orbital_Video_D3D12_SwapChain_ResolveRenderTexture(handle, sourceRenderTexture.GetHandle());
		}

		public override void CopyTexture(Texture2DBase sourceTexture)
		{
			Orbital_Video_D3D12_SwapChain_CopyTexture(handle, sourceTexture.GetHandle());
		}

		public override void CopyTexture(Texture2DBase sourceTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel)
		{
			Orbital_Video_D3D12_SwapChain_CopyTextureRegion(handle, sourceTexture.GetHandle(), sourceOffset.x, sourceOffset.y, destinationOffset.x, destinationOffset.y, size.width, size.height, sourceMipmapLevel);
		}

		#region Create Methods
		public override RenderPassBase CreateRenderPass(RenderPassDesc desc)
		{
			var abstraction = new RenderPass(deviceD3D12);
			if (!abstraction.Init(desc, this))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderPass");
			}
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc, DepthStencilBase depthStencil)
		{
			var abstraction = new RenderPass(deviceD3D12);
			if (!abstraction.Init(desc, this, (DepthStencil)depthStencil))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create RenderPass");
			}
			return abstraction;
		}
		#endregion
	}
}
