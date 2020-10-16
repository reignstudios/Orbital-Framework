using System;

namespace Orbital.Video.D3D12
{
	public sealed class RenderTexture2D : Texture2D
	{
		public readonly RenderTextureUsage usage;
		public DepthStencil depthStencil { get; private set; }

		public RenderTexture2D(Device device, RenderTextureUsage usage, TextureMode mode)
		: base(device, mode)
		{
			isRenderTexture = true;
			this.usage = usage;
		}

		public bool Init(int width, int height, TextureFormat format, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return Init(width, height, format, null, true, allowRandomAccess, msaaLevel, nodeVisibility);
		}

		public override bool Init(int width, int height, TextureFormat format, byte[] data, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			return Init(width, height, format, data, true, false, MSAALevel.Disabled, nodeVisibility);
		}

		public bool Init(int width, int height, TextureFormat format, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, MSAALevel msaaLevel, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			depthStencil = new DepthStencil(deviceD3D12, stencilUsage, depthStencilMode);
			if (!depthStencil.Init(width, height, depthStencilFormat, msaaLevel)) return false;
			return Init(width, height, format, null, true, allowRandomAccess, msaaLevel, nodeVisibility);
		}

		public bool Init(int width, int height, TextureFormat format, byte[] data, StencilUsage stencilUsage, DepthStencilFormat depthStencilFormat, DepthStencilMode depthStencilMode, bool allowRandomAccess, MultiGPUNodeResourceVisibility nodeVisibility)
		{
			depthStencil = new DepthStencil(deviceD3D12, stencilUsage, depthStencilMode);
			if (!depthStencil.Init(width, height, depthStencilFormat, MSAALevel.Disabled)) return false;
			return Init(width, height, format, data, true, allowRandomAccess, MSAALevel.Disabled, nodeVisibility);
		}

		public override void Dispose()
		{
			if (depthStencil != null)
			{
				depthStencil.Dispose();
				depthStencil = null;
			}
			base.Dispose();
		}

		#region RenderTexture Methods
		public override DepthStencilBase GetDepthStencil()
		{
			return depthStencil;
		}

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
