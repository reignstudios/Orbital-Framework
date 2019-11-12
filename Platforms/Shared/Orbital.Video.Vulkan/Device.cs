using System;
using System.Runtime.InteropServices;
using Orbital.Host;

namespace Orbital.Video.Vulkan
{
	public struct DeviceDesc
	{
		public int adapterIndex;
		public bool softwareRasterizer;
		public FeatureLevel minimumFeatureLevel;
		public WindowBase window;
		public bool ensureSwapChainMatchesWindowSize;
		public int swapChainBufferCount;
		public bool fullscreen;
	}

	public sealed class Device : DeviceBase
	{
		public Device(Instance instance, DeviceType type)
		: base(instance, type)
		{
			
		}

		public bool Init(DeviceDesc desc)
		{
			return false;
		}

		public override void Dispose()
		{
			
		}

		public override void BeginFrame()
		{
			
		}

		public override void EndFrame()
		{
			
		}

		public override void ExecuteCommandBuffer(CommandBufferBase commandBuffer)
		{
			
		}

		#region Create Methods
		public override SwapChainBase CreateSwapChain(WindowBase window, int bufferCount, bool fullscreen)
		{
			return null;
		}

		public override CommandBufferBase CreateCommandBuffer()
		{
			return null;
		}
		#endregion
	}
}
