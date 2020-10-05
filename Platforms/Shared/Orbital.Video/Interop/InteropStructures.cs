using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

#if D3D12
namespace Orbital.Video.D3D12
#elif VULKAN
namespace Orbital.Video.Vulkan
#endif
{
	enum ReadWriteBufferType
	{
		Texture
	}

	#region Render Pass
	[StructLayout(LayoutKind.Sequential)]
	struct RenderPassRenderTargetDesc_NativeInterop
	{
		public byte clearColor;
		public Color4F clearColorValue;

		public RenderPassRenderTargetDesc_NativeInterop(ref RenderPassRenderTargetDesc desc)
		{
			clearColor = (byte)(desc.clearColor ? 1 : 0);
			clearColorValue = desc.clearColorValue;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RenderPassDepthStencilDesc_NativeInterop
	{
		public byte clearDepth;
		public byte clearStencil;
		public float depthValue;
		public float stencilValue;

		public RenderPassDepthStencilDesc_NativeInterop(ref RenderPassDepthStencilDesc desc)
		{
			clearDepth = (byte)(desc.clearDepth ? 1 : 0);
			clearStencil = (byte)(desc.clearStencil ? 1 : 0);
			depthValue = desc.depthValue;
			stencilValue = desc.stencilValue;
		}
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
				renderTargetDescs[i] = new RenderPassRenderTargetDesc_NativeInterop(ref desc.renderTargetDescs[i]);
			}

			depthStencilDesc = new RenderPassDepthStencilDesc_NativeInterop(ref desc.depthStencilDesc);
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
		public int readWriteBufferCount;
		public IntPtr* readWriteBuffers;
		public ReadWriteBufferType* readWriteTypes;
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

			if (desc.readWriteBuffers != null)
			{
				readWriteBufferCount = desc.readWriteBuffers.Length;
				readWriteBuffers = (IntPtr*)Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>() * readWriteBufferCount);
				readWriteTypes = (ReadWriteBufferType*)Marshal.AllocHGlobal(sizeof(ReadWriteBufferType) * readWriteBufferCount);
				for (int i = 0; i != readWriteBufferCount; ++i)
				{
					var buffer = desc.readWriteBuffers[i];
					var type = buffer.GetType();
					if (typeof(Texture2DBase).IsAssignableFrom(type))
					{
						readWriteBuffers[i] = ((Texture2DBase)buffer).GetHandle();
						readWriteTypes[i] = ReadWriteBufferType.Texture;
					}
					else
					{
						throw new NotSupportedException("Unsupported ComputeShader Read/Write buffer type: " + type.ToString());
					}
				}
			}
			else
			{
				readWriteBufferCount = 0;
				readWriteBuffers = null;
				readWriteTypes = null;
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

			if (textureDepthStencils != null)
			{
				Marshal.FreeHGlobal((IntPtr)textureDepthStencils);
				textureDepthStencils = null;
			}

			if (readWriteTypes != null)
			{
				Marshal.FreeHGlobal((IntPtr)readWriteTypes);
				readWriteTypes = null;
			}

			blendDesc.Dispose();
		}
	}
	#endregion

	#region Compute State
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ComputeStateDesc_NativeInterop : IDisposable
	{
		public IntPtr computeShader;
		public int constantBufferCount;
		public IntPtr* constantBuffers;
		public int textureCount;
		public IntPtr* textures;
		public int textureDepthStencilCount;
		public IntPtr* textureDepthStencils;
		public int readWriteBufferCount;
		public IntPtr* readWriteBuffers;
		public ReadWriteBufferType* readWriteTypes;

		public ComputeStateDesc_NativeInterop(ref ComputeStateDesc desc)
		{
			computeShader = ((ComputeShader)desc.computeShader).handle;

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

			if (desc.readWriteBuffers != null)
			{
				readWriteBufferCount = desc.readWriteBuffers.Length;
				readWriteBuffers = (IntPtr*)Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>() * readWriteBufferCount);
				readWriteTypes = (ReadWriteBufferType*)Marshal.AllocHGlobal(sizeof(ReadWriteBufferType) * readWriteBufferCount);
				for (int i = 0; i != readWriteBufferCount; ++i)
				{
					var buffer = desc.readWriteBuffers[i];
					var type = buffer.GetType();
					if (typeof(Texture2DBase).IsAssignableFrom(type))
					{
						readWriteBuffers[i] = ((Texture2DBase)buffer).GetHandle();
						readWriteTypes[i] = ReadWriteBufferType.Texture;
					}
					else
					{
						throw new NotSupportedException("Unsupported ComputeShader Read/Write buffer type: " + type.ToString());
					}
				}
			}
			else
			{
				readWriteBufferCount = 0;
				readWriteBuffers = null;
				readWriteTypes = null;
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

			if (textureDepthStencils != null)
			{
				Marshal.FreeHGlobal((IntPtr)textureDepthStencils);
				textureDepthStencils = null;
			}

			if (readWriteTypes != null)
			{
				Marshal.FreeHGlobal((IntPtr)readWriteTypes);
				readWriteTypes = null;
			}
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

		public VertexBufferStreamDesc_NativeInterop(ref VertexBufferStreamDesc desc)
		{
			vertexBuffer = ((VertexBuffer)desc.vertexBuffer).handle;
			type = desc.type;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct VertexBufferStreamElement_NativeInterop
	{
		public int index;
		public VertexBufferStreamElementType type;
		public VertexBufferStreamElementUsage usage;
		public int usageIndex;
		public int offset;

		public VertexBufferStreamElement_NativeInterop(ref VertexBufferStreamElement element)
		{
			index = element.index;
			type = element.type;
			usage = element.usage;
			usageIndex = element.usageIndex;
			offset = element.offset;
		}
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
					descs[i] = new VertexBufferStreamDesc_NativeInterop(ref layout.descs[i]);
				}
			}

			// init elements
			if (layout.elements != null)
			{
				elementCount = layout.elements.Length;
				elements = (VertexBufferStreamElement_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<VertexBufferStreamElement_NativeInterop>() * elementCount);
				for (int i = 0; i != elementCount; ++i)
				{
					elements[i] = new VertexBufferStreamElement_NativeInterop(ref layout.elements[i]);
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
	struct ShaderSampler_NativeInterop
	{
		public int registerIndex;
		public ShaderSamplerFilter filter;
		public ShaderSamplerAnisotropy anisotropy;
		public ShaderSamplerAddress addressU, addressV, addressW;
		public ShaderComparisonFunction comparisonFunction;

		public ShaderSampler_NativeInterop(ref ShaderSampler sampler)
		{
			registerIndex = sampler.registerIndex;
			filter = sampler.filter;
			addressU = sampler.addressU;
			addressV = sampler.addressV;
			addressW = sampler.addressW;
			anisotropy = sampler.anisotropy;
			comparisonFunction = sampler.comparisonFunction;
		}
	}
	#endregion

	#region ShaderEffect
	[StructLayout(LayoutKind.Sequential)]
	struct ShaderEffectConstantBuffer_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectResourceUsage usage;

		public ShaderEffectConstantBuffer_NativeInterop(ref ShaderEffectConstantBuffer buffer)
		{
			registerIndex = buffer.registerIndex;
			usage = buffer.usage;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ShaderEffectTexture_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectResourceUsage usage;

		public ShaderEffectTexture_NativeInterop(ref ShaderEffectTexture texture)
		{
			registerIndex = texture.registerIndex;
			usage = texture.usage;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ShaderEffectReadWriteBuffer_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectResourceUsage usage;

		public ShaderEffectReadWriteBuffer_NativeInterop(ref ShaderEffectReadWriteBuffer buffer)
		{
			registerIndex = buffer.registerIndex;
			usage = buffer.usage;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ShaderEffectDesc_NativeInterop : IDisposable
	{
		public int constantBufferCount, textureCount, samplersCount, readWriteBufferCount;
		public ShaderEffectConstantBuffer_NativeInterop* constantBuffers;
		public ShaderEffectTexture_NativeInterop* textures;
		public ShaderSampler_NativeInterop* samplers;
		public ShaderEffectReadWriteBuffer_NativeInterop* readWriteBuffers;

		public ShaderEffectDesc_NativeInterop(ref ShaderEffectDesc desc)
		{
			// init defaults
			constantBufferCount = 0;
			textureCount = 0;
			samplersCount = 0;
			readWriteBufferCount = 0;
			constantBuffers = null;
			textures = null;
			samplers = null;
			readWriteBuffers = null;

			// allocate constant buffer heaps
			if (desc.constantBuffers != null)
			{
				constantBufferCount = desc.constantBuffers.Length;
				constantBuffers = (ShaderEffectConstantBuffer_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectConstantBuffer_NativeInterop>() * constantBufferCount);
				for (int i = 0; i != constantBufferCount; ++i)
				{
					constantBuffers[i] = new ShaderEffectConstantBuffer_NativeInterop(ref desc.constantBuffers[i]);
				}
			}

			// allocate texture heaps
			if (desc.textures != null)
			{
				textureCount = desc.textures.Length;
				textures = (ShaderEffectTexture_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectTexture_NativeInterop>() * textureCount);
				for (int i = 0; i != textureCount; ++i)
				{
					textures[i] = new ShaderEffectTexture_NativeInterop(ref desc.textures[i]);
				}
			}

			// allocate sampler heaps
			if (desc.samplers != null)
			{
				samplersCount = desc.samplers.Length;
				samplers = (ShaderSampler_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderSampler_NativeInterop>() * samplersCount);
				for (int i = 0; i != samplersCount; ++i)
				{
					samplers[i] = new ShaderSampler_NativeInterop(ref desc.samplers[i]);
				}
			}

			// allocate read-write-buffer heaps
			if (desc.readWriteBuffers != null)
			{
				readWriteBufferCount = desc.readWriteBuffers.Length;
				readWriteBuffers = (ShaderEffectReadWriteBuffer_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectReadWriteBuffer_NativeInterop>() * readWriteBufferCount);
				for (int i = 0; i != readWriteBufferCount; ++i)
				{
					readWriteBuffers[i] = new ShaderEffectReadWriteBuffer_NativeInterop(ref desc.readWriteBuffers[i]);
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

			if (readWriteBuffers != null)
			{
				Marshal.FreeHGlobal((IntPtr)readWriteBuffers);
				readWriteBuffers = null;
			}
		}
	}
	#endregion

	#region ComputeShader
	[StructLayout(LayoutKind.Sequential)]
	struct ComputeShaderConstantBuffer_NativeInterop
	{
		public int registerIndex;

		public ComputeShaderConstantBuffer_NativeInterop(ref ComputeShaderConstantBuffer buffer)
		{
			registerIndex = buffer.registerIndex;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ComputeShaderTexture_NativeInterop
	{
		public int registerIndex;

		public ComputeShaderTexture_NativeInterop(ref ComputeShaderTexture texture)
		{
			registerIndex = texture.registerIndex;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ComputeShaderReadWriteBuffer_NativeInterop
	{
		public int registerIndex;

		public ComputeShaderReadWriteBuffer_NativeInterop(ref ComputeShaderReadWriteBuffer buffer)
		{
			registerIndex = buffer.registerIndex;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ComputeShaderDesc_NativeInterop : IDisposable
	{
		public int constantBufferCount, textureCount, samplersCount, readWriteBufferCount;
		public ComputeShaderConstantBuffer_NativeInterop* constantBuffers;
		public ComputeShaderTexture_NativeInterop* textures;
		public ShaderSampler_NativeInterop* samplers;
		public ComputeShaderReadWriteBuffer_NativeInterop* readWriteBuffers;

		public ComputeShaderDesc_NativeInterop(ref ComputeShaderDesc desc)
		{
			// init defaults
			constantBufferCount = 0;
			textureCount = 0;
			samplersCount = 0;
			readWriteBufferCount = 0;
			constantBuffers = null;
			textures = null;
			samplers = null;
			readWriteBuffers = null;

			// allocate constant buffer heaps
			if (desc.constantBuffers != null)
			{
				constantBufferCount = desc.constantBuffers.Length;
				constantBuffers = (ComputeShaderConstantBuffer_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ComputeShaderConstantBuffer_NativeInterop>() * constantBufferCount);
				for (int i = 0; i != constantBufferCount; ++i)
				{
					constantBuffers[i] = new ComputeShaderConstantBuffer_NativeInterop(ref desc.constantBuffers[i]);
				}
			}

			// allocate texture heaps
			if (desc.textures != null)
			{
				textureCount = desc.textures.Length;
				textures = (ComputeShaderTexture_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ComputeShaderTexture_NativeInterop>() * textureCount);
				for (int i = 0; i != textureCount; ++i)
				{
					textures[i] = new ComputeShaderTexture_NativeInterop(ref desc.textures[i]);
				}
			}

			// allocate sampler heaps
			if (desc.samplers != null)
			{
				samplersCount = desc.samplers.Length;
				samplers = (ShaderSampler_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderSampler_NativeInterop>() * samplersCount);
				for (int i = 0; i != samplersCount; ++i)
				{
					samplers[i] = new ShaderSampler_NativeInterop(ref desc.samplers[i]);
				}
			}

			// allocate read-write-buffer heaps
			if (desc.readWriteBuffers != null)
			{
				readWriteBufferCount = desc.readWriteBuffers.Length;
				readWriteBuffers = (ComputeShaderReadWriteBuffer_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ComputeShaderReadWriteBuffer_NativeInterop>() * readWriteBufferCount);
				for (int i = 0; i != readWriteBufferCount; ++i)
				{
					readWriteBuffers[i] = new ComputeShaderReadWriteBuffer_NativeInterop(ref desc.readWriteBuffers[i]);
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

			if (readWriteBuffers != null)
			{
				Marshal.FreeHGlobal((IntPtr)readWriteBuffers);
				readWriteBuffers = null;
			}
		}
	}
	#endregion
}
