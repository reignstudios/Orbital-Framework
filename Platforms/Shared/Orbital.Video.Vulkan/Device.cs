using System;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.Vulkan
{
	public struct DeviceDesc
	{
		/// <summary>
		/// Represents "physical device group" index if Vulkan API 1.1 or newer. Otherwise "physical device" index
		/// </summary>
		public int adapterIndex;

		/// <summary>
		/// Window to the device will present to. Can be null for background devices
		/// </summary>
		public WindowBase window;

		/// <summary>
		/// If the window size changes, auto resize the swap-chain to match
		/// </summary>
		public bool ensureSwapChainMatchesWindowSize;

		/// <summary>
		/// Double/Tripple buffering etc
		/// </summary>
		public int swapChainBufferCount;

		/// <summary>
		/// True to launch in fullscreen
		/// </summary>
		public bool fullscreen;
	}

	public sealed class Device : DeviceBase
	{
		public readonly Instance instanceVulkan;
		internal IntPtr handle;
		internal SwapChain swapChain;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_Vulkan_Device_Create(IntPtr Instance, DeviceType type);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_Vulkan_Device_Init(IntPtr handle, int adapterIndex);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_BeginFrame(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_Vulkan_Device_EndFrame(IntPtr handle);

		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			instanceVulkan = instance;
			handle = Orbital_Video_Vulkan_Device_Create(instance.handle, type);
		}

		public bool Init(DeviceDesc desc)
		{
			if (Orbital_Video_Vulkan_Device_Init(handle, desc.adapterIndex) == 0) return false;
			if (type == DeviceType.Presentation)
			{
				swapChain = new SwapChain(this, desc.ensureSwapChainMatchesWindowSize);
				return swapChain.Init(desc.window, desc.swapChainBufferCount, desc.fullscreen);
			}
			else
			{
				return true;
			}
		}

		public override void Dispose()
		{
			if (swapChain != null)
			{
				swapChain.Dispose();
				swapChain = null;
			}

			if (handle != IntPtr.Zero)
			{
				Orbital_Video_Vulkan_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void BeginFrame()
		{
			if (type == DeviceType.Presentation) swapChain.BeginFrame();
			Orbital_Video_Vulkan_Device_BeginFrame(handle);
		}

		public override void EndFrame()
		{
			Orbital_Video_Vulkan_Device_EndFrame(handle);
			if (type == DeviceType.Presentation) swapChain.Present();
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen, bool ensureSwapChainMatchesWindowSize)
		{
			var abstraction = new SwapChain(this, ensureSwapChainMatchesWindowSize);
			if (!abstraction.Init(window, bufferCount, fullscreen))
			{
				abstraction.Dispose();
				throw new Exception("Failed to create SwapChain");
			}
			return abstraction;
		}

		public override CommandListBase CreateCommandList()
		{
			var abstraction = new CommandList(this);
			if (!abstraction.Init())
			{
				abstraction.Dispose();
				throw new Exception("Failed to create CommandList");
			}
			return abstraction;
		}

		public override RenderPassBase CreateRenderPass(RenderPassDesc desc)
		{
			return swapChain.CreateRenderPass(desc);
		}

		public override ShaderEffectBase CreateShaderEffect(Stream stream)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
