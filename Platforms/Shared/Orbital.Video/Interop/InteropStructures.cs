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
	struct RenderPassDesc_NativeInterop
	{
		public byte clearColor, clearDepthStencil;
		public Color4F clearColorValue;
		public float depthValue, stencilValue;

		public RenderPassDesc_NativeInterop(ref RenderPassDesc desc)
		{
			clearColor = (byte)(desc.clearColor ? 1 : 0);
			clearDepthStencil = (byte)(desc.clearDepthStencil ? 1 : 0);
			clearColorValue = desc.clearColorValue;
			depthValue = desc.depthValue;
			stencilValue = desc.stencilValue;
		}
	}
	#endregion

	#region Render State
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct RenderStateDesc_NativeInterop : IDisposable
	{
		public IntPtr renderPass;
		public IntPtr shaderEffect;
		public int constantBufferCount;
		public IntPtr* constantBuffers;
		public int textureCount;
		public IntPtr* textures;
		public VertexBufferTopology vertexBufferTopology;
		public IntPtr vertexBufferStreamer;
		public IntPtr indexBuffer;
		public byte depthEnable, stencilEnable;
		public TriangleCulling triangleCulling;
		public TriangleFillMode triangleFillMode;
		public MSAALevel msaaLevel;

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

			vertexBufferTopology = desc.vertexBufferTopology;
			vertexBufferStreamer = ((VertexBufferStreamer)desc.vertexBufferStreamer).handle;
			if (desc.indexBuffer != null) indexBuffer = ((IndexBuffer)desc.indexBuffer).handle;
			else indexBuffer = IntPtr.Zero;
			depthEnable = (byte)(desc.depthEnable ? 1 : 0);
			stencilEnable = (byte)(desc.stencilEnable ? 1 : 0);
			triangleCulling = desc.triangleCulling;
			triangleFillMode = desc.triangleFillMode;
			msaaLevel = desc.msaaLevel;
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
