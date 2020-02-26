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
		/// No operation is performed on the render target
		/// </summary>
		NoOperation,

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
		/// Factor 1 used in blending operation
		/// </summary>
		public BlendFactor factor1;

		/// <summary>
		/// Factor 2 used in blending operation
		/// </summary>
		public BlendFactor factor2;

		/// <summary>
		/// Blending operation between factors
		/// </summary>
		public BlendOperation operation;

		/// <summary>
		/// Alpha-Factor 1 used in blending operation
		/// </summary>
		public BlendFactor alphaFactor1;

		/// <summary>
		/// Alpha-Factor 2 used in blending operation
		/// </summary>
		public BlendFactor alphaFactor2;

		/// <summary>
		/// Blending operation between alpha-factors
		/// </summary>
		public BlendOperation alphaOperation;

		/// <summary>
		/// Logical operation which produces: 'result = (source OP destination)'
		/// </summary>
		public LogicalBlendOperation logicalOperation;
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

			if (desc.indexBuffer == null)
			{
				var vertexBuffer = desc.vertexBufferStreamer.vertexBuffers[0];
				desc.indexBuffer = vertexBuffer.indexBuffer;
			}
		}
	}
}
