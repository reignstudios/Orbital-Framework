using System;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Video.D3D12
{
	[StructLayout(LayoutKind.Sequential)]
	struct RenderPassDescNative
	{
		public byte clearColor, clearDepthStencil;
		public Vec4 clearColorValue;
		public float depthValue, stencilValue;
	}

	public sealed class RenderPass : RenderPassBase
	{
		internal IntPtr handle;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(IntPtr device, IntPtr swapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_RenderPass_Init(IntPtr handle, RenderPassDescNative* desc);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_RenderPass_Dispose(IntPtr handle);

		public RenderPass(SwapChain swapChain)
		{
			handle = Orbital_Video_D3D12_RenderPass_Create_WithSwapChain(swapChain.deviceD3D12.handle, swapChain.handle);
		}

		public unsafe bool Init(RenderPassDesc desc)
		{
			var descNative = new RenderPassDescNative()
			{
				clearColor = (byte)(desc.clearColor ? 1 : 0),
				clearDepthStencil = (byte)(desc.clearDepthStencil ? 1 : 0),
				clearColorValue = desc.clearColorValue,
				depthValue = desc.depthValue,
				stencilValue = desc.stencilValue
			};
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
