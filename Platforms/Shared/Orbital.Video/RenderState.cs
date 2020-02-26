using System;

namespace Orbital.Video
{
	public enum MSAALevel
	{
		Disabled = 0,
		X2 = 2,
		X4 = 4,
		X8 = 8,
		X16 = 16
	}

	public enum TriangleCulling
	{
		/// <summary>
		/// Double-sided. Both faces will draw
		/// </summary>
		None,

		/// <summary>
		/// Back faces will not draw
		/// </summary>
		Back,

		/// <summary>
		/// Front faces will not draw
		/// </summary>
		Front
	}

	public enum TriangleFillMode
	{
		/// <summary>
		/// Triangles entire surface will draw
		/// </summary>
		Solid,

		/// <summary>
		/// Triangle edges will draw as lines
		/// </summary>
		Wireframe
	}

	public enum BlendFactor
	{
		/// <summary>
		/// 0 as factor value
		/// </summary>
		Zero,

		/// <summary>
		/// 1 as factor value
		/// </summary>
		One,

		/// <summary>
		/// Source color
		/// </summary>
		SourceColor,

		/// <summary>
		/// Source color inverted
		/// </summary>
		SourceColorInverse,

		/// <summary>
		/// Source alpha
		/// </summary>
		SourceAlpha,

		/// <summary>
		/// Source alpha inverted
		/// </summary>
		SourceAlphaInverse,

		/// <summary>
		/// Destination color
		/// </summary>
		DestinationColor,

		/// <summary>
		/// Destination color inverted
		/// </summary>
		DestinationColorInverse,

		/// <summary>
		/// Destination alpha
		/// </summary>
		DestinationAlpha,

		/// <summary>
		/// Destination alpha inverted
		/// </summary>
		DestinationAlphaInverse,

		/// <summary>
		/// Pre blend operation which performs: 'factor = min(srcAlpha * srcColor, 1 - (srcAlpha * dstColor))'
		/// </summary>
		SourceAlphaSaturate,

		/// <summary>
		/// Second color source output from a pixel shader to a second render-target
		/// </summary>
		SourceColor2,

		/// <summary>
		/// Second color source output inverted from a pixel shader to a second render-target
		/// </summary>
		SourceColorInverse2,

		/// <summary>
		/// Second alpha source output from a pixel shader to a second render-target
		/// </summary>
		SourceAlpha2,

		/// <summary>
		/// Second alpha source output inverted from a pixel shader to a second render-target
		/// </summary>
		SourceAlphaInverse2
	}

	public enum BlendOperation
	{
		/// <summary>
		/// Add factor 1 and factor 2
		/// </summary>
		Add,

		/// <summary>
		/// Subtract factor 1 from factor 2
		/// </summary>
		Subtract,

		/// <summary>
		/// Subtract factor 2 from factor 1
		/// </summary>
		SubtractReversed,

		/// <summary>
		/// Find the minimum of factor 1 and factor 2
		/// </summary>
		Minimum,

		/// <summary>
		/// Find the maximum of factor 1 and factor 2
		/// </summary>
		Maximum
	}

	public enum LogicalBlendOperation
	{
		/// <summary>
		/// No operation is performed on the render target
		/// </summary>
		NoOperation,

		/// <summary>
		/// Clears the render target to 0
		/// </summary>
		Clear,

		/// <summary>
		/// Sets the render target to 1
		/// </summary>
		Set,

		/// <summary>
		/// Copys the render target
		/// </summary>
		Copy,

		/// <summary>
		/// Performs an inverted-copy of the render target
		/// </summary>
		CopyInverted,

		/// <summary>
		/// Inverts the render target
		/// </summary>
		Invert,

		/// <summary>
		/// Performs a logical AND operation on the render target
		/// </summary>
		AND,

		/// <summary>
		/// Performs a logical NAND operation on the render target
		/// </summary>
		NAND,

		/// <summary>
		/// Performs a logical OR operation on the render target
		/// </summary>
		OR,
		
		/// <summary>
		/// Performs a logical NOR operation on the render target
		/// </summary>
		NOR,

		/// <summary>
		/// Performs a logical XOR operation on the render target
		/// </summary>
		XOR,

		/// <summary>
		/// Performs a logical equal operation on the render target producing 0 or 1
		/// </summary>
		Equivalent,

		/// <summary>
		/// Performs a logical AND and reverse operation on the render target
		/// </summary>
		AND_Reverse,

		/// <summary>
		/// Performs a logical AND and invert operation on the render target
		/// </summary>
		AND_Inverted,

		/// <summary>
		/// Performs a logical OR and reverse operation on the render target
		/// </summary>
		OR_Reverse,

		/// <summary>
		/// Performs a logical OR and invert operation on the render target
		/// </summary>
		OR_Inverted
	}

	[Flags]
	public enum BlendWriteMask
	{
		/// <summary>
		/// Write red color channel
		/// </summary>
		Red = 1,

		/// <summary>
		/// Write green color channel
		/// </summary>
		Green = 2,

		/// <summary>
		/// Write blue color channel
		/// </summary>
		Blue = 4,

		/// <summary>
		/// Write alpha color channel
		/// </summary>
		Alpha = 8,

		/// <summary>
		/// Write all color channels
		/// </summary>
		All = Red | Green | Blue | Alpha
	}

	public struct RenderTargetBlendDesc
	{
		/// <summary>
		/// Enables blending.
		/// Must be set to false if logicOperationEnabled is true
		/// </summary>
		public bool blendingEnabled;
		
		/// <summary>
		/// Enables logical operations.
		/// Must be set to false if blendingEnabled is true
		/// </summary>
		public bool logicOperationEnabled;

		/// <summary>
		/// Alpha channel uses its own blending factors and operation
		/// </summary>
		public bool alphaBlendingSeparated;

		/// <summary>
		/// Source factor used in blending operation
		/// </summary>
		public BlendFactor sourceFactor;

		/// <summary>
		/// Destination factor used in blending operation
		/// </summary>
		public BlendFactor destinationFactor;

		/// <summary>
		/// Blending operation between factors 'result = ((src * srcFactor) OP (dst * dstFactor))'
		/// </summary>
		public BlendOperation operation;

		/// <summary>
		/// Source Alpha-Factor used in blending operation
		/// </summary>
		public BlendFactor sourceAlphaFactor;

		/// <summary>
		/// Destination Alpha-Factor used in blending operation
		/// </summary>
		public BlendFactor destinationAlphaFactor;

		/// <summary>
		/// Blending operation between alpha-factors 'result = ((srcAlpha * srcAlphaFactor) OP (dstAlpha * dstAlphaFactor))'
		/// </summary>
		public BlendOperation alphaOperation;

		/// <summary>
		/// Logical operation which produces: 'result = (source OP destination)'
		/// </summary>
		public LogicalBlendOperation logicalOperation;

		/// <summary>
		/// What color channels to write
		/// </summary>
		public BlendWriteMask writeMask;

		public static RenderTargetBlendDesc BlendingDisabled()
		{
			return new RenderTargetBlendDesc()
			{
				blendingEnabled = false,
				writeMask = BlendWriteMask.All
			};
		}

		public static RenderTargetBlendDesc AlphaBlending()
		{
			return new RenderTargetBlendDesc()
			{
				blendingEnabled = true,
				sourceFactor = BlendFactor.SourceAlpha,
				destinationFactor = BlendFactor.SourceAlphaInverse,
				operation = BlendOperation.Add,
				writeMask = BlendWriteMask.All
			};
		}

		public static RenderTargetBlendDesc AdditiveBlending()
		{
			return new RenderTargetBlendDesc()
			{
				blendingEnabled = true,
				sourceFactor = BlendFactor.One,
				destinationFactor = BlendFactor.One,
				operation = BlendOperation.Add,
				writeMask = BlendWriteMask.All
			};
		}

		public static RenderTargetBlendDesc SubtractiveBlending()
		{
			return new RenderTargetBlendDesc()
			{
				blendingEnabled = true,
				sourceFactor = BlendFactor.One,
				destinationFactor = BlendFactor.One,
				operation = BlendOperation.Subtract,
				writeMask = BlendWriteMask.All
			};
		}
	}

	public struct BlendDesc
	{
		/// <summary>
		/// Enable to allow MSAA anti-alias blending in alpha-clipped textures
		/// </summary>
		public bool alphaToCoverageEnable;
		
		/// <summary>
		/// True to enable unique blending descriptions/operations per render target.
		/// Otherwise only first render target will use blending if enabled.
		/// This must be set to false if logicOperationEnabled is true
		/// </summary>
		public bool independentBlendEnable;

		/// <summary>
		/// Render target blend descriptions
		/// </summary>
		public RenderTargetBlendDesc[] renderTargetBlendDescs;
	}

	public struct RenderStateDesc
	{
		/// <summary>
		/// Render pass this state will be used in
		/// </summary>
		public RenderPassBase renderPass;

		/// <summary>
		/// Shader effect to render geometry with
		/// </summary>
		public ShaderEffectBase shaderEffect;

		/// <summary>
		/// Constant buffers to be accessed in shader effect
		/// </summary>
		public ConstantBufferBase[] constantBuffers;

		/// <summary>
		/// Textures to be accessed in shader effect
		/// </summary>
		public TextureBase[] textures;

		/// <summary>
		/// How the geometry will appear
		/// </summary>
		public VertexBufferTopology vertexBufferTopology;

		/// <summary>
		/// Vertex buffers to use and stream in parallel
		/// </summary>
		public VertexBufferStreamerBase vertexBufferStreamer;

		/// <summary>
		/// Index buffer to use.
		/// If null, the IndexBuffer from the first element of the VertexBufferStreamer will be used
		/// </summary>
		public IndexBufferBase indexBuffer;

		/// <summary>
		/// Enables depth read/write
		/// </summary>
		public bool depthEnable;
		
		/// <summary>
		/// Enables stencil read/write
		/// </summary>
		public bool stencilEnable;

		/// <summary>
		/// Vertex buffers face culling method to use
		/// </summary>
		public TriangleCulling triangleCulling;

		/// <summary>
		/// Vertex buffers fill mode to use
		/// </summary>
		public TriangleFillMode triangleFillMode;

		/// <summary>
		/// Multisample anti-aliasing level
		/// </summary>
		public MSAALevel msaaLevel;

		/// <summary>
		/// Blending description
		/// </summary>
		public BlendDesc blendDesc;
	}

	public abstract class RenderStateBase : IDisposable
	{
		public readonly DeviceBase device;

		public RenderStateBase(DeviceBase device)
		{
			this.device = device;
		}

		public abstract void Dispose();

		protected void InitBase(ref RenderStateDesc desc)
		{
			int constantBufferCount = desc.constantBuffers != null ? desc.constantBuffers.Length : 0;
			if (desc.shaderEffect.constantBufferCount != constantBufferCount) throw new ArgumentException("RenderState constant-buffer count doesn't match ShaderEffect requirements");

			int textureCount = desc.textures != null ? desc.textures.Length : 0;
			if (desc.shaderEffect.textureCount != textureCount) throw new ArgumentException("RenderState texture count doesn't match ShaderEffect requirements");

			// validate independentBlendEnable is true settings
			if (desc.blendDesc.independentBlendEnable)
			{
				// validate render-target blend descriptions length matches render-pass render-target count
				if (desc.blendDesc.renderTargetBlendDescs == null || desc.blendDesc.renderTargetBlendDescs.Length != desc.renderPass.renderTargetCount)
				{
					throw new ArgumentException("'independentBlendEnable' requires 'renderTargetBlendDescs' length to match RenderPass render-target count");
				}

				// validate 'logicOperationEnabled' is not enabled when 'independentBlendEnable' is
				if (desc.blendDesc.renderTargetBlendDescs != null)
				{
					foreach (var blendDesc in desc.blendDesc.renderTargetBlendDescs)
					{
						if (blendDesc.logicOperationEnabled) throw new ArgumentException("'logicOperationEnabled' cannot be enabled when 'independentBlendEnable' is enabled");
					}
				}
			}

			// validate 'renderTargetBlendDescs' is null or length = 1 when 'independentBlendEnable' is disabled
			if (!desc.blendDesc.independentBlendEnable && desc.blendDesc.renderTargetBlendDescs != null && desc.blendDesc.renderTargetBlendDescs.Length != 1)
			{
				throw new ArgumentException("'independentBlendEnable' set to false requires 'renderTargetBlendDescs' length to equal 1");
			}

			// validate both 'blendingEnabled' and 'logicOperationEnabled' are not enabled at the same time
			if (desc.blendDesc.renderTargetBlendDescs != null)
			{
				foreach (var blendDesc in desc.blendDesc.renderTargetBlendDescs)
				{
					if (blendDesc.blendingEnabled && blendDesc.logicOperationEnabled) throw new ArgumentException("Only 'blendingEnabled' or 'logicOperationEnabled' can be enabled not both");
				}
			}

			// if index-buffer override is null, use built in vertex-buffer one
			if (desc.indexBuffer == null)
			{
				var vertexBuffer = desc.vertexBufferStreamer.vertexBuffers[0];
				desc.indexBuffer = vertexBuffer.indexBuffer;
			}
		}
	}
}
