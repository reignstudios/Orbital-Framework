using System;

namespace Orbital.Video.D3D12
{
	public sealed class RenderTexture2D : Texture2D
	{
		public readonly RenderTextureUsage usage;

		public RenderTexture2D(Device device, RenderTextureUsage usage, TextureMode mode)
		: base(device, mode)
		{
			isRenderTexture = true;
			this.usage = usage;
		}

		public bool Init(TextureFormat format, int width, int height)
		{
			return Init(format, width, height, null, true);
		}

		public override bool Init(TextureFormat format, int width, int height, byte[] data)
		{
			return Init(format, width, height, data, true);
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
