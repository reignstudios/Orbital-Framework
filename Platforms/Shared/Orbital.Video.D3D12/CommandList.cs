using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class CommandList : CommandListBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		private RenderPass lastRenderPass;
		private RenderState lastRenderState;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_CommandList_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_CommandList_Init(IntPtr handle, CommandListType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Start(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Finish(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_BeginRenderPass(IntPtr handle, IntPtr renderPass);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_EndRenderPass(IntPtr handle, IntPtr renderPass);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(IntPtr handle, IntPtr swapChain, float r, float g, float b, float a);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_SetViewPort(IntPtr handle, uint x, uint y, uint width, uint height, float minDepth, float maxDepth);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_SetRenderState(IntPtr handle, IntPtr renderState);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_SetComputeState(IntPtr handle, IntPtr computeState);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_ExecuteComputeShader(IntPtr handle, uint threadGroupCountX, uint threadGroupCountY, uint threadGroupCountZ);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_DrawInstanced(IntPtr handle, uint vertexOffset, uint vertexCount, uint instanceCount);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(IntPtr handle, uint vertexOffset, uint indexOffset, uint indexCount, uint instanceCount);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_ResolveRenderTexture(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstRenderTexture);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_ResolveRenderTextureToSwapChain(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstSwapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_CopyTexture(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstRenderTexture);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_CopyTextureToSwapChain(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstSwapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_CopyTextureRegion(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstRenderTexture, uint srcX, uint srcY, uint srcZ, uint dstX, uint dstY, uint dstZ, uint width, uint height, uint depth, uint srcMipmapLevel, uint dstMipmapLevel);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_CopyTextureToSwapChainRegion(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstSwapChain, uint srcX, uint srcY, uint srcZ, uint dstX, uint dstY, uint dstZ, uint width, uint height, uint depth, uint srcMipmapLevel);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Execute(IntPtr handle);

		internal CommandList(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_CommandList_Create(device.handle);
		}

		public bool Init(CommandListType type)
		{
			return Orbital_Video_D3D12_CommandList_Init(handle, type) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_CommandList_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Start()
		{
			Orbital_Video_D3D12_CommandList_Start(handle);
		}

		public override void Finish()
		{
			Orbital_Video_D3D12_CommandList_Finish(handle);
			lastRenderState = null;
		}

		public override void BeginRenderPass(RenderPassBase renderPass)
		{
			lastRenderPass = (RenderPass)renderPass;
			Orbital_Video_D3D12_CommandList_BeginRenderPass(handle, lastRenderPass.handle);
		}

		public override void EndRenderPass()
		{
			Orbital_Video_D3D12_CommandList_EndRenderPass(handle, lastRenderPass.handle);
			lastRenderPass = null;
		}

		public override void ClearRenderTarget(float r, float g, float b, float a)
		{
			Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(handle, deviceD3D12.swapChainD3D12.handle, r, b, g, a);
		}

		public override void ClearRenderTarget(SwapChainBase swapChain, float r, float g, float b, float a)
		{
			var swapChainD3D12 = (SwapChain)swapChain;
			Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(handle, swapChainD3D12.handle, r, b, g, a);
		}

		public override void ClearRenderTarget(Texture2DBase renderTexture, float r, float g, float b, float a)
		{
			#if DEBUG
			if (!renderTexture.isRenderTexture) throw new Exception("Is not render-texture");
			#endif
			throw new NotImplementedException();
		}

		public override void SetViewPort(ViewPort viewPort)
		{
			Orbital_Video_D3D12_CommandList_SetViewPort(handle, (uint)viewPort.rect.position.x, (uint)viewPort.rect.position.y, (uint)viewPort.rect.size.width, (uint)viewPort.rect.size.height, viewPort.minDepth, viewPort.maxDepth);
		}

		public override void SetRenderState(RenderStateBase renderState)
		{
			lastRenderState = (RenderState)renderState;
			Orbital_Video_D3D12_CommandList_SetRenderState(handle, lastRenderState.handle);
		}

		public override void SetComputeState(ComputeStateBase computeState)
		{
			var computeStateD3D12 = (ComputeState)computeState;
			Orbital_Video_D3D12_CommandList_SetComputeState(handle, computeStateD3D12.handle);
		}

		public override void Draw()
		{
			if (lastRenderState.indexCount == 0) Orbital_Video_D3D12_CommandList_DrawInstanced(handle, 0, (uint)lastRenderState.vertexCount, 1);
			else Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(handle, 0, 0, (uint)lastRenderState.indexCount, 1);
		}

		public override void ExecuteComputeShader(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
		{
			Orbital_Video_D3D12_CommandList_ExecuteComputeShader(handle, (uint)threadGroupCountX, (uint)threadGroupCountY, (uint)threadGroupCountZ);
		}

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture, Texture2DBase destinationRenderTexture)
		{
			var src = (RenderTexture2D)sourceRenderTexture;
			var dst = (RenderTexture2D)destinationRenderTexture;
			Orbital_Video_D3D12_CommandList_ResolveRenderTexture(handle, src.handle, dst.handle);
		}

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture, SwapChainBase destinationSwapChain)
		{
			var src = (RenderTexture2D)sourceRenderTexture;
			var dst = (SwapChain)destinationSwapChain;
			Orbital_Video_D3D12_CommandList_ResolveRenderTextureToSwapChain(handle, src.handle, dst.handle);
		}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture)
		{
			var src = (RenderTexture2D)sourceTexture;
			var dst = (RenderTexture2D)destinationTexture;
			Orbital_Video_D3D12_CommandList_CopyTexture(handle, src.handle, dst.handle);
		}

		public override void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain)
		{
			var src = (RenderTexture2D)sourceTexture;
			var dst = (SwapChain)destinationSwapChain;
			Orbital_Video_D3D12_CommandList_CopyTextureToSwapChain(handle, src.handle, dst.handle);
		}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel, int destinationMipmapLevel)
		{
			var src = (RenderTexture2D)sourceTexture;
			var dst = (RenderTexture2D)destinationTexture;
			Orbital_Video_D3D12_CommandList_CopyTextureRegion(handle, src.handle, dst.handle, (uint)sourceOffset.x, (uint)sourceOffset.y, 0, (uint)destinationOffset.x, (uint)destinationOffset.y, 0, (uint)size.width, (uint)size.height, 1, (uint)sourceMipmapLevel, (uint)destinationMipmapLevel);
		}

		public override void CopyTexture(Texture2DBase sourceTexture, SwapChainBase destinationSwapChain, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel)
		{
			var src = (RenderTexture2D)sourceTexture;
			var dst = (SwapChain)destinationSwapChain;
			Orbital_Video_D3D12_CommandList_CopyTextureToSwapChainRegion(handle, src.handle, dst.handle, (uint)sourceOffset.x, (uint)sourceOffset.y, 0, (uint)destinationOffset.x, (uint)destinationOffset.y, 0, (uint)size.width, (uint)size.height, 1, (uint)sourceMipmapLevel);
		}

		public override void Execute()
		{
			Orbital_Video_D3D12_CommandList_Execute(handle);
		}
	}
}
