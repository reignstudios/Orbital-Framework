using Orbital.Numerics;
using System;

namespace Orbital.Video
{
	public struct RenderPassRenderTargetDesc
	{
		/// <summary>
		/// Clear render-target if true
		/// </summary>
		public bool clearColor;

		/// <summary>
		/// Color to clear render-target with
		/// </summary>
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
		/// <summary>
		/// Clear depth if true
		/// </summary>
		public bool clearDepth;

		/// <summary>
		/// Clear stencil if true
		/// </summary>
		public bool clearStencil;

		/// <summary>
		/// 0-1 depth value to clear depth with.
		/// This is normally set to 1.
		/// </summary>
		public float depthValue;
		
		/// <summary>
		/// 0-1 stencil value to clear stencil with.
		/// This is normally set to 255.
		/// </summary>
		public float stencilValue;

		/// <summary>
		/// Creates default settings for standard depth testing.
		/// </summary>
		/// <param name="clearDepth">True to clear depth</param>
		public static RenderPassDepthStencilDesc CreateDefault(bool clearDepth)
		{
			return new RenderPassDepthStencilDesc()
			{
				clearDepth = clearDepth,
				depthValue = 1,
				stencilValue = 1
			};
		}

		/// <summary>
		/// Creates default settings for standard depth and stencil testing.
		/// </summary>
		/// <param name="clearDepth">True to clear depth</param>
		/// <param name="clearStencil">True to clear stencil</param>
		public static RenderPassDepthStencilDesc CreateDefault(bool clearDepth, bool clearStencil)
		{
			return new RenderPassDepthStencilDesc()
			{
				clearDepth = clearDepth,
				clearStencil = clearStencil,
				depthValue = 1,
				stencilValue = 1
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
