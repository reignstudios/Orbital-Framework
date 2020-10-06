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
		/// Back faces will not draw. Counter-Clockwise faces.
		/// </summary>
		Back,

		/// <summary>
		/// Front faces will not draw. Clockwise faces.
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

		/// <summary>
		/// Disables blending
		/// </summary>
		public static RenderTargetBlendDesc BlendingDisabled()
		{
			var result = new RenderTargetBlendDesc()
			{
				blendingEnabled = false,
				writeMask = BlendWriteMask.All
			};
			return result;
		}

		/// <summary>
		/// Standard alpha blending
		/// </summary>
		public static RenderTargetBlendDesc AlphaBlending()
		{
			var result = new RenderTargetBlendDesc()
			{
				blendingEnabled = true,
				sourceFactor = BlendFactor.SourceAlpha,
				destinationFactor = BlendFactor.SourceAlphaInverse,
				operation = BlendOperation.Add,
				writeMask = BlendWriteMask.All
			};
			return result;
		}

		/// <summary>
		/// Standard additive blending
		/// </summary>
		public static RenderTargetBlendDesc AdditiveBlending()
		{
			var result = new RenderTargetBlendDesc()
			{
				blendingEnabled = true,
				sourceFactor = BlendFactor.One,
				destinationFactor = BlendFactor.One,
				operation = BlendOperation.Add,
				writeMask = BlendWriteMask.All
			};
			return result;
		}

		/// <summary>
		/// Standard subtractive blending
		/// </summary>
		public static RenderTargetBlendDesc SubtractiveBlending()
		{
			var result = new RenderTargetBlendDesc()
			{
				blendingEnabled = true,
				sourceFactor = BlendFactor.One,
				destinationFactor = BlendFactor.One,
				operation = BlendOperation.Subtract,
				writeMask = BlendWriteMask.All
			};
			return result;
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

	public enum DepthStencilTestFunction
	{
		/// <summary>
		/// Always pass the comparison
		/// </summary>
		Always,

		/// <summary>
		/// Never pass the comparison
		/// </summary>
		Never,

		/// <summary>
		/// If the source data is equal to the destination data, the comparison passes
		/// </summary>
		Equal,

		/// <summary>
		/// If the source data is not equal to the destination data, the comparison passes
		/// </summary>
		NotEqual,

		/// <summary>
		/// If the source data is less than the destination data, the comparison passes
		/// </summary>
		LessThan,

		/// <summary>
		/// If the source data is less than or equal to the destination data, the comparison passes
		/// </summary>
		LessThanOrEqual,

		/// <summary>
		/// If the source data is greater than the destination data, the comparison passes
		/// </summary>
		GreaterThan,

		/// <summary>
		/// If the source data is greater than or equal to the destination data, the comparison passes
		/// </summary>
		GreaterThanOrEqual
	}

	public enum StencilOperation
	{
		/// <summary>
		/// Keep the existing stencil data
		/// </summary>
		Keep,

		/// <summary>
		/// Set the stencil data to 0
		/// </summary>
		Zero,

		/// <summary>
		/// Invert the stencil data
		/// </summary>
		Invert,

		/// <summary>
		/// Increment the stencil value by 1, and wrap the result if necessary
		/// </summary>
		IncrementWrap,

		/// <summary>
		/// Decrement the stencil value by 1, and wrap the result if necessary
		/// </summary>
		DecrementWrap,

		/// <summary>
		/// Increment the stencil value by 1, and clamp the result
		/// </summary>
		IncrementClamp,

		/// <summary>
		/// Decrement the stencil value by 1, and clamp the result
		/// </summary>
		DecrementClamp
	}

	public struct StencilTestOperationDesc
	{
		/// <summary>
		/// Depth clipping function to use when 'stencilTestEnable' is enabled
		/// </summary>
		public DepthStencilTestFunction stencilTestFunction;

		/// <summary>
		/// The stencil operation to perform when stencil testing and depth testing both pass
		/// </summary>
		public StencilOperation stencilPassOperation;

		/// <summary>
		/// The stencil operation to perform when stencil testing fails
		/// </summary>
		public StencilOperation stencilFailOperation;

		/// <summary>
		/// The stencil operation to perform when stencil testing passes and depth testing fails
		/// </summary>
		public StencilOperation stencilDepthFailOperation;
	}

	public struct DepthStencilDesc
	{
		/// <summary>
		/// Enables depth read and test-function
		/// </summary>
		public bool depthTestEnable;

		/// <summary>
		/// Enables depth write
		/// </summary>
		public bool depthWriteEnable;

		/// <summary>
		/// Clipping function to use when 'depthTestEnable' is enabled
		/// </summary>
		public DepthStencilTestFunction depthTestFunction;

		/// <summary>
		/// Enables stencil read and test-function
		/// </summary>
		public bool stencilTestEnable;

		/// <summary>
		/// Enables stencil write
		/// </summary>
		public bool stencilWriteEnable;

		/// <summary>
		/// Stencil description for front facing operations. Clockwise faces.
		/// </summary>
		public StencilTestOperationDesc stencilFrontFacingDesc;

		/// <summary>
		/// Stencil description for back facing operations. Counter-Clockwise faces.
		/// </summary>
		public StencilTestOperationDesc stencilBackFacingDesc;

		/// <summary>
		/// Standard depth testing. Stencil disabled.
		/// </summary>
		public static DepthStencilDesc StandardDepthTesting()
		{
			var result = new DepthStencilDesc()
			{
				depthTestEnable = true,
				depthWriteEnable = true,
				depthTestFunction = DepthStencilTestFunction.LessThan
			};
			return result;
		}

		/// <summary>
		/// Basic stencil clipping. Depth disabled.
		/// This assumes the stencil was cleared with 'RenderPassDepthStencilDesc.stencilValue = 1'
		/// </summary>
		/// <param name="stencilClip">True to clip or false to write clipping mask</param>
		/// <param name="doubleSidedFacing">False to only use front-faces or True for both front and back faces</param>
		public static DepthStencilDesc BasicStencilTesting(bool stencilClip, bool doubleSidedFacing)
		{
			var result = new DepthStencilDesc()
			{
				stencilTestEnable = stencilClip,
				stencilWriteEnable = !stencilClip
			};
			
			result.stencilFrontFacingDesc.stencilTestFunction = stencilClip ? DepthStencilTestFunction.LessThan : DepthStencilTestFunction.Always;
			result.stencilFrontFacingDesc.stencilFailOperation = StencilOperation.Keep;
			result.stencilFrontFacingDesc.stencilDepthFailOperation = StencilOperation.Keep;
			result.stencilFrontFacingDesc.stencilPassOperation = stencilClip ? StencilOperation.Keep : StencilOperation.Zero;
			if (doubleSidedFacing)
			{
				result.stencilBackFacingDesc = result.stencilFrontFacingDesc;
			}
			else
			{
				result.stencilBackFacingDesc.stencilTestFunction = DepthStencilTestFunction.Never;
				result.stencilBackFacingDesc.stencilFailOperation = StencilOperation.Keep;
				result.stencilBackFacingDesc.stencilDepthFailOperation = StencilOperation.Keep;
				result.stencilBackFacingDesc.stencilPassOperation = StencilOperation.Keep;
			}

			return result;
		}
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
		/// Depth-Stencils to use as texture resources.
		/// NOTE: Register indicies will come after 'textures'.
		/// </summary>
		public DepthStencilBase[] textureDepthStencils;

		/// <summary>
		/// Random access buffers to be accessed in compute shader
		/// </summary>
		public object[] randomAccessBuffers;

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
		/// Vertex buffers face culling method to use
		/// </summary>
		public TriangleCulling triangleCulling;

		/// <summary>
		/// Vertex buffers fill mode to use
		/// </summary>
		public TriangleFillMode triangleFillMode;

		/// <summary>
		/// Blending description
		/// </summary>
		public BlendDesc blendDesc;

		/// <summary>
		/// Depth-Stencil description
		/// </summary>
		public DepthStencilDesc depthStencilDesc;
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
			if (desc.shaderEffect.constantBufferCount != constantBufferCount) throw new ArgumentException("RenderStateDesc constant-buffer count doesn't match ShaderEffect requirements");

			int textureCount = desc.textures != null ? desc.textures.Length : 0;
			if (desc.shaderEffect.textureCount != textureCount) throw new ArgumentException("RenderStateDesc texture count doesn't match ShaderEffect requirements");

			int randomAccessBufferCount = desc.randomAccessBuffers != null ? desc.randomAccessBuffers.Length : 0;
			if (desc.shaderEffect.randomAccessBufferCount != randomAccessBufferCount) throw new ArgumentException("RenderStateDesc random access buffer count doesn't match ShaderEffect requirements");

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
