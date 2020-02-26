using Orbital.Numerics;
using System;

namespace Orbital.Video
{
	public struct RenderPassRenderTargetDesc
	{
		public bool clearColor;
		public Color4F clearColorValue;

		public static RenderPassRenderTargetDesc CreateDefault(Color4F clearColorValue)
		{
			return new RenderPassRenderTargetDesc()
			{
				clearColor = true,
				clearColorValue = clearColorValue
			};
		}
	}

	public struct RenderPassDepthStencilDesc
	{
		public bool clearDepth, clearStencil;
		public float depthValue, stencilValue;

		public static RenderPassDepthStencilDesc CreateDefault(bool clearDepth)
		{
			return new RenderPassDepthStencilDesc()
			{
				clearDepth = clearDepth,
				depthValue = 1
			};
		}

		public static RenderPassDepthStencilDesc CreateDefault(bool clearDepth, bool clearStencil)
		{
			return new RenderPassDepthStencilDesc()
			{
				clearDepth = clearDepth,
				clearStencil = clearStencil,
				depthValue = 1
			};
		}
	}

	public struct RenderPassDesc
	{
		public RenderPassRenderTargetDesc[] renderTargetDescs;
		public RenderPassDepthStencilDesc depthStencilDesc;

		public static RenderPassDesc CreateDefault(int renderTargetCount)
		{
			var result = new RenderPassDesc()
			{
				renderTargetDescs = new RenderPassRenderTargetDesc[renderTargetCount],
				depthStencilDesc = RenderPassDepthStencilDesc.CreateDefault(true)
			};
			for (int i = 0; i != renderTargetCount; ++i) result.renderTargetDescs[i] = RenderPassRenderTargetDesc.CreateDefault(Color4F.black);
			return result;
		}

		public static RenderPassDesc CreateDefault(Color4F clearColorValue, int renderTargetCount)
		{
			var result = new RenderPassDesc()
			{
				renderTargetDescs = new RenderPassRenderTargetDesc[renderTargetCount],
				depthStencilDesc = RenderPassDepthStencilDesc.CreateDefault(true)
			};
			for (int i = 0; i != renderTargetCount; ++i) result.renderTargetDescs[i] = RenderPassRenderTargetDesc.CreateDefault(clearColorValue);
			return result;
		}

		public static RenderPassDesc CreateDefault(Color4F clearColorValue, int renderTargetCount, bool clearDepthStencil)
		{
			var result = new RenderPassDesc()
			{
				renderTargetDescs = new RenderPassRenderTargetDesc[renderTargetCount],
				depthStencilDesc = RenderPassDepthStencilDesc.CreateDefault(clearDepthStencil)
			};
			for (int i = 0; i != renderTargetCount; ++i) result.renderTargetDescs[i] = RenderPassRenderTargetDesc.CreateDefault(clearColorValue);
			return result;
		}
	}

	public abstract class RenderPassBase : IDisposable
	{
		public readonly DeviceBase device;
		public int renderTargetCount { get; private set; }

		public RenderPassBase(DeviceBase device)
		{
			this.device = device;
		}

		protected void InitBase(ref RenderPassDesc desc, int renderTargetCount)
		{
			if (desc.renderTargetDescs == null) throw new Exception("Must contain 'renderTargetDescs'");
			if (desc.renderTargetDescs.Length != renderTargetCount) throw new Exception("'renderTargetDescs' length must match render targets length");
			this.renderTargetCount = renderTargetCount;
		}

		public abstract void Dispose();
	}
}
