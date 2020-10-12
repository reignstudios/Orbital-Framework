using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class RasterizeCommandList : RasterizeCommandListBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;

		internal RasterizeCommandList(Device device)
		: base(device)
		{
			deviceVulkan = device;
			handle = CommandList.Orbital_Video_Vulkan_CommandList_Create(device.handle);
		}

		public bool Init()
		{
			return CommandList.Orbital_Video_Vulkan_CommandList_Init(handle, CommandListType.Rasterize) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				CommandList.Orbital_Video_Vulkan_CommandList_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Start(int nodeIndex)
		{
			CommandList.Orbital_Video_Vulkan_CommandList_Start(handle);
		}

		public override void Finish()
		{
			CommandList.Orbital_Video_Vulkan_CommandList_Finish(handle);
		}

		public override void BeginRenderPass(RenderPassBase renderPass)
		{
			var renderPassVulkan = (RenderPass)renderPass;
			CommandList.Orbital_Video_Vulkan_CommandList_BeginRenderPass(handle, renderPassVulkan.handle);
		}

		public override void EndRenderPass()
		{
			CommandList.Orbital_Video_Vulkan_CommandList_EndRenderPass(handle);
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

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture, Texture2DBase destinationRenderTexture)
		{
			throw new NotImplementedException();
		}

		//public override void ResolveMSAA(Texture2DBase sourceRenderTexture, SwapChainBase destinationSwapChain)
		//{
		//	throw new NotImplementedException();
		//}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture)
		{
			throw new NotImplementedException();
		}

		//public override void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain)
		//{
		//	throw new NotImplementedException();
		//}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel, int destinationMipmapLevel)
		{
			throw new NotImplementedException();
		}

		//public override void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel)
		//{
		//	throw new NotImplementedException();
		//}

		public override void Execute()
		{
			CommandList.Orbital_Video_Vulkan_CommandList_Execute(handle);
		}
	}
}
