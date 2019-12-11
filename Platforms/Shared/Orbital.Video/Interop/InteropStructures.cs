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
		public Vec4 clearColorValue;
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
	public unsafe struct RenderStateDesc_NativeInterop
	{
		public IntPtr renderPass;
		public IntPtr shaderEffect;
		public IntPtr vertexBuffer;
		public VertexBufferTopology vertexBufferTopology;
		public int msaaLevel;

		public RenderStateDesc_NativeInterop(ref RenderStateDesc desc)
		{
			renderPass = ((RenderPass)desc.renderPass).handle;
			shaderEffect = ((ShaderEffect)desc.shaderEffect).handle;
			vertexBuffer = ((VertexBuffer)desc.vertexBuffer).handle;
			vertexBufferTopology = desc.vertexBufferTopology;
			msaaLevel = desc.msaaLevel;
		}
	}
	#endregion

	#region Vertex Buffer
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexBufferLayoutElement_NativeInterop
	{
		public VertexBufferLayoutElementType type;
		public VertexBufferLayoutElementUsage usage;
		public int streamIndex, usageIndex, byteOffset;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct VertexBufferLayout_NativeInterop : IDisposable
	{
		public int elementCount;
		public VertexBufferLayoutElement_NativeInterop* elements;

		public VertexBufferLayout_NativeInterop(ref VertexBufferLayout layout)
		{
			// init defaults
			elementCount = 0;
			elements = null;

			// init elements
			if (layout.elements != null)
			{
				elementCount = layout.elements.Length;
				elements = (VertexBufferLayoutElement_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<VertexBufferLayoutElement_NativeInterop>() * elementCount);
				for (int i = 0; i != elementCount; ++i)
				{
					elements[i].type = layout.elements[i].type;
					elements[i].usage = layout.elements[i].usage;
					elements[i].streamIndex = layout.elements[i].streamIndex;
					elements[i].usageIndex = layout.elements[i].usageIndex;
					elements[i].byteOffset = layout.elements[i].byteOffset;
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
	unsafe struct ShaderEffectResource_NativeInterop
	{
		public int registerIndex;
		public int usedInTypesCount;
		public ShaderType* usedInTypes;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct ShaderEffectSampler_NativeInterop
	{
		public int registerIndex;
		public ShaderEffectSampleFilter filter;
		public ShaderEffectSampleAddress addressU, addressV, addressW;
		public ShaderEffectSamplerAnisotropy anisotropy;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct ShaderEffectConstantBuffer_NativeInterop
	{
		public int registerIndex;
		public int size;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ShaderEffectDesc_NativeInterop : IDisposable
	{
		public int resourcesCount, samplersCount, constantBufferCount;
		public ShaderEffectResource_NativeInterop* resources;
		public ShaderEffectSampler_NativeInterop* samplers;
		public ShaderEffectConstantBuffer_NativeInterop* constantBuffers;

		public ShaderEffectDesc_NativeInterop(ref ShaderEffectDesc desc)
		{
			// init defaults
			resourcesCount = 0;
			samplersCount = 0;
			constantBufferCount = 0;
			resources = null;
			samplers = null;
			constantBuffers = null;

			// allocate resource heaps
			if (desc.resources != null)
			{
				resourcesCount = desc.resources.Length;
				resources = (ShaderEffectResource_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectResource_NativeInterop>() * resourcesCount);
				for (int i = 0; i != resourcesCount; ++i)
				{
					resources[i].registerIndex = desc.resources[i].registerIndex;
					if (desc.resources[i].usedInTypes != null)
					{
						resources[i].usedInTypesCount = desc.resources[i].usedInTypes.Length;
						long size = sizeof(ShaderType) * resources[i].usedInTypesCount;
						resources[i].usedInTypes = (ShaderType*)Marshal.AllocHGlobal((int)size);
						fixed (ShaderType* usedInTypesPtr = desc.resources[i].usedInTypes)
						{
							Buffer.MemoryCopy(usedInTypesPtr, resources[i].usedInTypes, size, size);
						}
					}
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

			// allocate constant buffer heaps
			if (desc.constantBuffers != null)
			{
				constantBufferCount = desc.constantBuffers.Length;
				constantBuffers = (ShaderEffectConstantBuffer_NativeInterop*)Marshal.AllocHGlobal(Marshal.SizeOf<ShaderEffectConstantBuffer_NativeInterop>() * constantBufferCount);
				for (int i = 0; i != constantBufferCount; ++i)
				{
					constantBuffers[i].registerIndex = desc.constantBuffers[i].registerIndex;
					constantBuffers[i].size = desc.constantBuffers[i].size;
				}
			}
		}

		public void Dispose()
		{
			if (resources != null)
			{
				for (int i = 0; i != resourcesCount; ++i)
				{
					if (resources[i].usedInTypes != null)
					{
						Marshal.FreeHGlobal((IntPtr)resources[i].usedInTypes);
						resources[i].usedInTypes = null;
					}
				}
				Marshal.FreeHGlobal((IntPtr)resources);
				resources = null;
			}

			if (samplers != null)
			{
				Marshal.FreeHGlobal((IntPtr)samplers);
				samplers = null;
			}

			if (constantBuffers != null)
			{
				Marshal.FreeHGlobal((IntPtr)constantBuffers);
				constantBuffers = null;
			}
		}
	}
	#endregion
}
