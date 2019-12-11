using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class CommandList : CommandListBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		private VertexBuffer lastVertexBuffer;
		private RenderPass lastRenderPass;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_CommandList_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_D3D12_CommandList_Init(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Start(IntPtr handle, IntPtr device);

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
		private static extern void Orbital_Video_D3D12_CommandList_SetVertexBuffer(IntPtr handle, IntPtr vertexBuffer);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_SetConstantBuffer(IntPtr handle, IntPtr constantBuffer, uint registerIndex);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_DrawInstanced(IntPtr handle, uint vertexIndex, uint vertexCount, uint instanceCount);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_CommandList_Execute(IntPtr handle);

		internal CommandList(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_CommandList_Create(device.handle);
		}

		public bool Init()
		{
			return Orbital_Video_D3D12_CommandList_Init(handle) != 0;
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
			Orbital_Video_D3D12_CommandList_Start(handle, deviceD3D12.handle);
		}

		public override void Finish()
		{
			Orbital_Video_D3D12_CommandList_Finish(handle);
			lastVertexBuffer = null;
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
			Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(handle, deviceD3D12.swapChain.handle, r, b, g, a);
		}

		public override void ClearRenderTarget(SwapChainBase swapChain, float r, float g, float b, float a)
		{
			var swapChainD3D12 = (SwapChain)swapChain;
			Orbital_Video_D3D12_CommandList_ClearSwapChainRenderTarget(handle, swapChainD3D12.handle, r, b, g, a);
		}

		public override void ClearRenderTarget(RenderTargetBase renderTarget, float r, float g, float b, float a)
		{
			throw new NotImplementedException();
		}

		public override void SetViewPort(ViewPort viewPort)
		{
			Orbital_Video_D3D12_CommandList_SetViewPort(handle, (uint)viewPort.rect.position.x, (uint)viewPort.rect.position.y, (uint)viewPort.rect.size.width, (uint)viewPort.rect.size.height, viewPort.minDepth, viewPort.maxDepth);
		}

		public override void SetRenderState(RenderStateBase renderState)
		{
			var renderStateD3D12 = (RenderState)renderState;
			lastVertexBuffer = renderStateD3D12.vertexBuffer;
			Orbital_Video_D3D12_CommandList_SetRenderState(handle, renderStateD3D12.handle);
		}

		public override void SetVertexBuffer(VertexBufferBase vertexBuffer)
		{
			lastVertexBuffer = (VertexBuffer)vertexBuffer;
			Orbital_Video_D3D12_CommandList_SetVertexBuffer(handle, lastVertexBuffer.handle);
		}

		public override void SetConstantBuffer(ConstantBufferBase constantBuffer, int registerIndex)
		{
			var constantBufferD3D12 = (ConstantBuffer)constantBuffer;
			Orbital_Video_D3D12_CommandList_SetConstantBuffer(handle, constantBufferD3D12.handle, (uint)registerIndex);
		}

		public override void Draw()
		{
			Orbital_Video_D3D12_CommandList_DrawInstanced(handle, 0, (uint)lastVertexBuffer.vertexCount, 1);
		}

		public override void Execute()
		{
			Orbital_Video_D3D12_CommandList_Execute(handle);
		}
	}
}
