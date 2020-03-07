using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

#if D3D12
namespace Orbital.Video.D3D12
#elif VULKAN
namespace Orbital.Video.Vulkan
#endif
{
	#region Render Pass
	[StructLayout(LayoutKind.Sequential)]
	struct RenderPassRenderTargetDesc_NativeInterop
	{
		public byte clearColor;
		public Color4F clearColorValue;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RenderPassDepthStencilDesc_NativeInterop
	{
		public byte clearDepth;
		public byte clearStencil;
		public float depthValue;
		public float stencilValue;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct RenderPassDesc_NativeInterop : IDisposable
	{
		public int renderTargetDescCount;
		public RenderPassRenderTargetDesc_NativeInterop* renderTargetDescs;
		public RenderPassDepthStencilDesc_NativeInterop depthStencilDesc;

		public RenderPassDesc_NativeInterop(ref RenderPassDesc desc)
		{
			renderTargetDescCount = desc.renderTargetDescs.Length;
			renderTargetDescs = (RenderPassRenderTargetDesc_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<RenderPassRenderTargetDesc_NativeInterop>() * desc.renderTargetDescs.Length);
			for (int i = 0; i != desc.renderTargetDescs.Length; ++i)
			{
				renderTargetDescs[i].clearColor = (byte)(desc.renderTargetDescs[i].clearColor ? 1 : 0);
				renderTargetDescs[i].clearColorValue = desc.renderTargetDescs[i].clearColorValue;
			}

			depthStencilDesc.clearDepth = (byte)(desc.depthStencilDesc.clearDepth ? 1 : 0);
			depthStencilDesc.clearStencil = (byte)(desc.depthStencilDesc.clearStencil ? 1 : 0);
			depthStencilDesc.depthValue = desc.depthStencilDesc.depthValue;
			depthStencilDesc.stencilValue = desc.depthStencilDesc.stencilValue;
		}

		public void Dispose()
		{
			if (renderTargetDescs != null)
			{
				Marshal.FreeHGlobal((IntPtr)renderTargetDescs);
				renderTargetDescs = null;
			}
		}
	}
	#endregion

	#region Render State
	[StructLayout(LayoutKind.Sequential)]
	struct RenderTargetBlendDesc_NativeInterop
	{
		public byte blendingEnabled;
		public byte logicOperationEnabled;
		public byte alphaBlendingSeparated;
		public BlendFactor sourceFactor;
		public BlendFactor destinationFactor;
		public BlendOperation operation;
		public BlendFactor sourceAlphaFactor;
		public BlendFactor destinationAlphaFactor;
		public BlendOperation alphaOperation;
		public LogicalBlendOperation logicalOperation;
		public BlendWriteMask writeMask;

		public RenderTargetBlendDesc_NativeInterop(ref RenderTargetBlendDesc desc)
		{
			blendingEnabled = (byte)(desc.blendingEnabled ? 1 : 0);
			logicOperationEnabled = (byte)(desc.logicOperationEnabled ? 1 : 0);
			alphaBlendingSeparated = (byte)(desc.alphaBlendingSeparated ? 1 : 0);
			sourceFactor = desc.sourceFactor;
			destinationFactor = desc.destinationFactor;
			operation = desc.operation;
			sourceAlphaFactor = desc.sourceAlphaFactor;
			destinationAlphaFactor = desc.destinationAlphaFactor;
			alphaOperation = desc.alphaOperation;
			logicalOperation = desc.logicalOperation;
			writeMask = desc.writeMask;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct BlendDesc_NativeInterop : IDisposable
	{
		public byte alphaToCoverageEnable;
		public byte independentBlendEnable;
		public int renderTargetBlendDescCount;
		public RenderTargetBlendDesc_NativeInterop* renderTargetBlendDescs;

		public BlendDesc_NativeInterop(ref BlendDesc desc)
		{
			alphaToCoverageEnable = (byte)(desc.alphaToCoverageEnable ? 1 : 0);
			independentBlendEnable = (byte)(desc.independentBlendEnable ? 1 : 0);
			if (desc.renderTargetBlendDescs != null)
			{
				renderTargetBlendDescCount = desc.renderTargetBlendDescs.Length;
				renderTargetBlendDescs = (RenderTargetBlendDesc_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<RenderTargetBlendDesc_NativeInterop>());
				for (int i = 0; i != renderTargetBlendDescCount; ++i) renderTargetBlendDescs[i] = new RenderTargetBlendDesc_NativeInterop(ref desc.renderTargetBlendDescs[i]);
			}
			else
			{
				renderTargetBlendDescCount = 0;
				renderTargetBlendDescs = null;
			}
		}

		public void Dispose()
		{
			if (renderTargetBlendDescs != null)
			{
				Marshal.FreeHGlobal((IntPtr)renderTargetBlendDescs);
				renderTargetBlendDescs = null;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct StencilTestOperationDesc_NativeInterop
	{
		public DepthStencilTestFunction stencilTestFunction;
		public StencilOperation stencilPassOperation;
		public StencilOperation stencilFailOperation;
		public StencilOperation stencilDepthFailOperation;

		public StencilTestOperationDesc_NativeInterop(ref StencilTestOperationDesc desc)
		{
			stencilTestFunction = desc.stencilTestFunction;
			stencilFailOperation = desc.stencilFailOperation;
			stencilDepthFailOperation = desc.stencilDepthFailOperation;
			stencilPassOperation = desc.stencilPassOperation;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct DepthStencilDesc_NativeInterop
	{
		public byte depthTestEnable;
		public byte depthWriteEnable;
		public DepthStencilTestFunction depthTestFunction;
		public byte stencilTestEnable;
		public byte stencilWriteEnable;
		public StencilTestOperationDesc_NativeInterop stencilFrontFacingDesc;
		public StencilTestOperationDesc_NativeInterop stencilBackFacingDesc;

		public DepthStencilDesc_NativeInterop(ref DepthStencilDesc desc)
		{
			depthTestEnable = (byte)(desc.depthTestEnable ? 1 : 0);
			depthWriteEnable = (byte)(desc.depthWriteEnable ? 1 : 0);
			depthTestFunction = desc.depthTestFunction;
			stencilTestEnable = (byte)(desc.stencilTestEnable ? 1 : 0);
			stencilWriteEnable = (byte)(desc.stencilWriteEnable ? 1 : 0);
			stencilFrontFacingDesc = new StencilTestOperationDesc_NativeInterop(ref desc.stencilFrontFacingDesc);
			stencilBackFacingDesc = new StencilTestOperationDesc_NativeInterop(ref desc.stencilBackFacingDesc);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct RenderStateDesc_NativeInterop : IDisposable
	{
		public IntPtr renderPass;
		public IntPtr shaderEffect;
		public int constantBufferCount;
		public IntPtr* constantBuffers;
		public int textureCount;
		public IntPtr* textures;
		public int textureDepthStencilCount;
		public IntPtr* textureDepthStencils;
		public VertexBufferTopology vertexBufferTopology;
		public IntPtr vertexBufferStreamer;
		public IntPtr indexBuffer;
		public TriangleCulling triangleCulling;
		public TriangleFillMode triangleFillMode;
		public BlendDesc_NativeInterop blendDesc;
		public DepthStencilDesc_NativeInterop depthStencilDesc;

		public RenderStateDesc_NativeInterop(ref RenderStateDesc desc)
		{
			renderPass = ((RenderPass)desc.renderPass).handle;
			shaderEffect = ((ShaderEffect)desc.shaderEffect).handle;

			if (desc.constantBuffers != null)
			{
				constantBufferCount = desc.constantBuffers.Length;
				constantBuffers = (IntPtr*)Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>() * constantBufferCount);
				for (int i = 0; i != constantBufferCount; ++i) constantBuffers[i] = ((ConstantBuffer)desc.constantBuffers[i]).handle;
			}
			else
			{
				constantBufferCount = 0;
				constantBuffers = null;
			}

			if (desc.textures != null)
			{
				textureCount = desc.textures.Length;
				textures = (IntPtr*)Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>() * textureCount);
				for (int i = 0; i != textureCount; ++i) textures[i] = desc.textures[i].GetHandle();
			}
			else
			{
				textureCount = 0;
				textures = null;
			}

			if (desc.textureDepthStencils != null)
			{
				textureDepthStencilCount = desc.textureDepthStencils.Length;
				textureDepthStencils = (IntPtr*)Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>() * textureDepthStencilCount);
				for (int i = 0; i != textureDepthStencilCount; ++i) textureDepthStencils[i] = desc.textureDepthStencils[i].GetHandle();
			}
			else
			{
				textureDepthStencilCount = 0;
				textureDepthStencils = null;
			}

			vertexBufferTopology = desc.vertexBufferTopology;
			vertexBufferStreamer = ((VertexBufferStreamer)desc.vertexBufferStreamer).handle;
			if (desc.indexBuffer != null) indexBuffer = ((IndexBuffer)desc.indexBuffer).handle;
			else indexBuffer = IntPtr.Zero;
			triangleCulling = desc.triangleCulling;
			triangleFillMode = desc.triangleFillMode;
			blendDesc = new BlendDesc_NativeInterop(ref desc.blendDesc);
			depthStencilDesc = new DepthStencilDesc_NativeInterop(ref desc.depthStencilDesc);
		}

		public void Dispose()
		{
			if (constantBuffers != null)
			{
				Marshal.FreeHGlobal((IntPtr)constantBuffers);
				constantBuffers = null;
			}

			if (textures != null)
			{
				Marshal.FreeHGlobal((IntPtr)textures);
				textures = null;
			}

			blendDesc.Dispose();
		}
	}
	#endregion

	#region Texture
	enum TextureType_NativeInterop
	{
		_1D,
		_2D,
		_3D,
		_Cube
	}
	#endregion

	#region Vertex Buffer
	[StructLayout(LayoutKind.Sequential)]
	struct VertexBufferStreamDesc_NativeInterop
	{
		public IntPtr vertexBuffer;
		public VertexBufferStreamType type;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct VertexBufferStreamElement_NativeInterop
	{
		public int index;
		public VertexBufferStreamElementType type;
		public VertexBufferStreamElementUsage usage;
		public int usageIndex;
		public int offset;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct VertexBufferStreamLayout_NativeInterop : IDisposable
	{
		public int descCount;
		public VertexBufferStreamDesc_NativeInterop* descs;

		public int elementCount;
		public VertexBufferStreamElement_NativeInterop* elements;

		public VertexBufferStreamLayout_NativeInterop(ref VertexBufferStreamLayout layout)
		{
			// init defaults
			descCount = 0;
			descs = null;
			elementCount = 0;
			elements = null;

			// init descriptions
			if (layout.descs != null)
			{
				descCount = layout.descs.Length;
				descs = (VertexBufferStreamDesc_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<VertexBufferStreamDesc_NativeInterop>() * descCount);
				for (int i = 0; i != descCount; ++i)
				{
					var vertexBuffer = (VertexBuffer)layout.descs[i].vertexBuffer;
					descs[i].vertexBuffer = vertexBuffer.handle;
					descs[i].type = layout.descs[i].type;
				}
			}

			// init elements
			if (layout.elements != null)
			{
				elementCount = layout.elements.Length;
				elements = (VertexBufferStreamElement_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<VertexBufferStreamElement_NativeInterop>() * elementCount);
				for (int i = 0; i != elementCount; ++i)
				{
					elements[i].index = layout.elements[i].index;
					elements[i].type = layout.elements[i].type;
					elements[i].usage = layout.elements[i].usage;
					elements[i].usageIndex = layout.elements[i].usageIndex;
					elements[i].offset = layout.elements[i].offset;
				}
			}
		}

		public void Dispose()
		{
			if (elements != null)
			{
				Marshal.FreeHGlobal((IntPtr)elements);
				elements = null;
			}
		}
	}
	#endregion

	#region Shaders
	[StructLayout(LayoutKind.Sequential)]
	struct ShaderEffectConstantBuffer_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectResourceUsage usage;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ShaderEffectTexture_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectResourceUsage usage;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct ShaderEffectSampler_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectSamplerFilter filter;
		public ShaderEffectSamplerAnisotropy anisotropy;
		public ShaderEffectSamplerAddress addressU, addressV, addressW;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ShaderEffectDesc_NativeInterop : IDisposable
	{
		public int constantBufferCount, textureCount, samplersCount;
		public ShaderEffectConstantBuffer_NativeInterop* constantBuffers;
		public ShaderEffectTexture_NativeInterop* textures;
		public ShaderEffectSampler_NativeInterop* samplers;

		public ShaderEffectDesc_NativeInterop(ref ShaderEffectDesc desc)
		{
			// init defaults
			constantBufferCount = 0;
			textureCount = 0;
			samplersCount = 0;
			constantBuffers = null;
			textures = null;
			samplers = null;

			// allocate constant buffer heaps
			if (desc.constantBuffers != null)
			{
				constantBufferCount = desc.constantBuffers.Length;
				constantBuffers = (ShaderEffectConstantBuffer_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectConstantBuffer_NativeInterop>() * constantBufferCount);
				for (int i = 0; i != constantBufferCount; ++i)
				{
					constantBuffers[i].registerIndex = desc.constantBuffers[i].registerIndex;
					constantBuffers[i].usage = desc.constantBuffers[i].usage;
				}
			}

			// allocate texture heaps
			if (desc.textures != null)
			{
				textureCount = desc.textures.Length;
				textures = (ShaderEffectTexture_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectTexture_NativeInterop>() * textureCount);
				for (int i = 0; i != textureCount; ++i)
				{
					textures[i].registerIndex = desc.textures[i].registerIndex;
					textures[i].usage = desc.textures[i].usage;
				}
			}

			// allocate sampler heaps
			if (desc.samplers != null)
			{
				samplersCount = desc.samplers.Length;
				samplers = (ShaderEffectSampler_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectSampler_NativeInterop>() * samplersCount);
				for (int i = 0; i != samplersCount; ++i)
				{
					samplers[i].registerIndex = desc.samplers[i].registerIndex;
					samplers[i].filter = desc.samplers[i].filter;
					samplers[i].addressU = desc.samplers[i].addressU;
					samplers[i].addressV = desc.samplers[i].addressV;
					samplers[i].addressW = desc.samplers[i].addressW;
					samplers[i].anisotropy = desc.samplers[i].anisotropy;
				}
			}
		}

		public void Dispose()
		{
			if (constantBuffers != null)
			{
				Marshal.FreeHGlobal((IntPtr)constantBuffers);
				constantBuffers = null;
			}

			if (textures != null)
			{
				Marshal.FreeHGlobal((IntPtr)textures);
				textures = null;
			}

			if (samplers != null)
			{
				Marshal.FreeHGlobal((IntPtr)samplers);
				samplers = null;
			}
		}
	}
	#endregion
}
