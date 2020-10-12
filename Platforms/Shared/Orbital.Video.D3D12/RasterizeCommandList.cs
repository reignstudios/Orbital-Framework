using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class RasterizeCommandList : RasterizeCommandListBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		private RenderPass lastRenderPass;
		private RenderState lastRenderState;

		internal RasterizeCommandList(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = CommandList.Orbital_Video_D3D12_CommandList_Create(device.handle);
		}

		public bool Init()
		{
			return CommandList.Orbital_Video_D3D12_CommandList_Init(handle, CommandListType.Rasterize) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				CommandList.Orbital_Video_D3D12_CommandList_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Start(int nodeIndex)
		{
			CommandList.Orbital_Video_D3D12_CommandList_Start(handle, nodeIndex);
		}

		public override void Finish()
		{
			CommandList.Orbital_Video_D3D12_CommandList_Finish(handle);
			lastRenderState = null;
		}

		public override void BeginRenderPass(RenderPassBase renderPass)
		{
			lastRenderPass = (RenderPass)renderPass;
			CommandList.Orbital_Video_D3D12_CommandList_BeginRenderPass(handle, lastRenderPass.handle);
		}

		public override void EndRenderPass()
		{
			CommandList.Orbital_Video_D3D12_CommandList_EndRenderPass(handle, lastRenderPass.handle);
			lastRenderPass = null;
		}

		public override void SetViewPort(ViewPort viewPort)
		{
			CommandList.Orbital_Video_D3D12_CommandList_SetViewPort(handle, (uint)viewPort.rect.position.x, (uint)viewPort.rect.position.y, (uint)viewPort.rect.size.width, (uint)viewPort.rect.size.height, viewPort.minDepth, viewPort.maxDepth);
		}

		public override void SetRenderState(RenderStateBase renderState)
		{
			lastRenderState = (RenderState)renderState;
			CommandList.Orbital_Video_D3D12_CommandList_SetRenderState(handle, lastRenderState.handle);
		}

		public override void Draw()
		{
			if (lastRenderState.indexCount == 0) CommandList.Orbital_Video_D3D12_CommandList_DrawInstanced(handle, 0, (uint)lastRenderState.vertexCount, 1);
			else CommandList.Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(handle, 0, 0, (uint)lastRenderState.indexCount, 1);
		}

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture, Texture2DBase destinationRenderTexture)
		{
			var src = (RenderTexture2D)sourceRenderTexture;
			var dst = (RenderTexture2D)destinationRenderTexture;
			CommandList.Orbital_Video_D3D12_CommandList_ResolveRenderTexture(handle, src.handle, dst.handle);
		}

		//public override void ResolveMSAA(Texture2DBase sourceRenderTexture, SwapChainBase destinationSwapChain)
		//{
		//	var src = (RenderTexture2D)sourceRenderTexture;
		//	var dst = (SwapChain)destinationSwapChain;
		//	CommandList.Orbital_Video_D3D12_CommandList_ResolveRenderTextureToSwapChain(handle, src.handle, dst.handle);
		//}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture)
		{
			var src = (RenderTexture2D)sourceTexture;
			var dst = (RenderTexture2D)destinationTexture;
			CommandList.Orbital_Video_D3D12_CommandList_CopyTexture(handle, src.handle, dst.handle);
		}

		//public override void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain)
		//{
		//	var src = (RenderTexture2D)sourceTexture;
		//	var dst = (SwapChain)destinationSwapChain;
		//	CommandList.Orbital_Video_D3D12_CommandList_CopyTextureToSwapChain(handle, src.handle, dst.handle);
		//}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel, int destinationMipmapLevel)
		{
			var src = (RenderTexture2D)sourceTexture;
			var dst = (RenderTexture2D)destinationTexture;
			CommandList.Orbital_Video_D3D12_CommandList_CopyTextureRegion(handle, src.handle, dst.handle, (uint)sourceOffset.x, (uint)sourceOffset.y, 0, (uint)destinationOffset.x, (uint)destinationOffset.y, 0, (uint)size.width, (uint)size.height, 1, (uint)sourceMipmapLevel, (uint)destinationMipmapLevel);
		}

		//public override void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel)
		//{
		//	var src = (RenderTexture2D)sourceTexture;
		//	var dst = (SwapChain)destinationSwapChain;
		//	CommandList.Orbital_Video_D3D12_CommandList_CopyTextureToSwapChainRegion(handle, src.handle, dst.handle, (uint)sourceOffset.x, (uint)sourceOffset.y, 0, (uint)destinationOffset.x, (uint)destinationOffset.y, 0, (uint)size.width, (uint)size.height, 1, (uint)sourceMipmapLevel);
		//}

		public override void Execute()
		{
			CommandList.Orbital_Video_D3D12_CommandList_Execute(handle);
		}
	}
}
