using Orbital.Numerics;

namespace Orbital.Video.API.mGPU
{
	public sealed class RasterizeCommandList : RasterizeCommandListBase
	{
		public readonly Device deviceMGPU;

		public RasterizeCommandList(Device device)
		: base(device)
		{
			deviceMGPU = device;
		}

		public bool Init()
		{
			return false;
		}

		public override void BeginRenderPass(RenderPassBase renderPass)
		{
			throw new System.NotImplementedException();
		}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture)
		{
			throw new System.NotImplementedException();
		}

		public override void CopyTexture(Texture2DBase sourceTexture, Texture2DBase destinationTexture, Point2 sourceOffset, Point2 destinationOffset, Size2 size, int sourceMipmapLevel, int destinationMipmapLevel)
		{
			throw new System.NotImplementedException();
		}

		public override void Dispose()
		{
			throw new System.NotImplementedException();
		}

		public override void Draw()
		{
			throw new System.NotImplementedException();
		}

		public override void EndRenderPass()
		{
			throw new System.NotImplementedException();
		}

		public override void Execute()
		{
			throw new System.NotImplementedException();
		}

		public override void Finish()
		{
			throw new System.NotImplementedException();
		}

		public override void ResolveMSAA(Texture2DBase sourceRenderTexture, Texture2DBase destinationRenderTexture)
		{
			throw new System.NotImplementedException();
		}

		public override void SetRenderState(RenderStateBase renderState)
		{
			throw new System.NotImplementedException();
		}

		public override void SetViewPort(ViewPort viewPort)
		{
			throw new System.NotImplementedException();
		}

		public override void Start(int nodeIndex)
		{
			throw new System.NotImplementedException();
		}
	}
}
