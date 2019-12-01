using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

#if D3D12
namespace Orbital.Video.D3D12
#elif VULKAN
namespace Orbital.Video.Vulkan
#endif
{
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
	unsafe struct ShaderEffectDesc_NativeInterop : IDisposable
	{
		public int resourcesCount, samplersCount;
		public ShaderEffectResource_NativeInterop* resources;
		public ShaderEffectSampler_NativeInterop* samplers;

		public ShaderEffectDesc_NativeInterop(ref ShaderEffectDesc desc)
		{
			// init defaults
			resourcesCount = 0;
			samplersCount = 0;
			resources = null;
			samplers = null;

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
		}
	}
}
