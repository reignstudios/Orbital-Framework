using System;
using System.Runtime.InteropServices;
using Orbital.Numerics;

namespace Orbital.Video.D3D12
{
	static class CommandList
	{
		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern IntPtr Orbital_Video_D3D12_CommandList_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern int Orbital_Video_D3D12_CommandList_Init(IntPtr handle, CommandListType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_Start(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_Finish(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_BeginRenderPass(IntPtr handle, IntPtr renderPass);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_EndRenderPass(IntPtr handle, IntPtr renderPass);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(IntPtr handle, IntPtr swapChain, float r, float g, float b, float a);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_SetViewPort(IntPtr handle, uint x, uint y, uint width, uint height, float minDepth, float maxDepth);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_SetRenderState(IntPtr handle, IntPtr renderState);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_SetComputeState(IntPtr handle, IntPtr computeState);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_ExecuteComputeShader(IntPtr handle, uint threadGroupCountX, uint threadGroupCountY, uint threadGroupCountZ);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_DrawInstanced(IntPtr handle, uint vertexOffset, uint vertexCount, uint instanceCount);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_DrawIndexedInstanced(IntPtr handle, uint vertexOffset, uint indexOffset, uint indexCount, uint instanceCount);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_ResolveRenderTexture(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstRenderTexture);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_ResolveRenderTextureToSwapChain(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstSwapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_CopyTexture(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstRenderTexture);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_CopyTextureToSwapChain(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstSwapChain);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_CopyTextureRegion(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstRenderTexture, uint srcX, uint srcY, uint srcZ, uint dstX, uint dstY, uint dstZ, uint width, uint height, uint depth, uint srcMipmapLevel, uint dstMipmapLevel);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_CopyTextureToSwapChainRegion(IntPtr handle, IntPtr srcRenderTexture, IntPtr dstSwapChain, uint srcX, uint srcY, uint srcZ, uint dstX, uint dstY, uint dstZ, uint width, uint height, uint depth, uint srcMipmapLevel);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		public static extern void Orbital_Video_D3D12_CommandList_Execute(IntPtr handle);
	}
}
