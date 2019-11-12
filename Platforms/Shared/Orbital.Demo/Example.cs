using System;
using System.IO;
using Orbital.Host;
using Orbital.Video;
using Orbital.Video.API;

namespace Orbital.Demo
{
	public sealed class Example : IDisposable
	{
		private ApplicationBase application;
		private WindowBase window;

		private InstanceBase instance;
		private DeviceBase device;
		private CommandBufferBase commandBuffer;

		public Example(ApplicationBase application, WindowBase window)
		{
			this.application = application;
			this.window = window;
		}

		public void Init(string platformPath, string folder64Bit, string folder32Bit)
		{
			// pre-load native libs
			string libFolderBit;
			if (IntPtr.Size == 8) libFolderBit = folder64Bit;
			else if (IntPtr.Size == 4) libFolderBit = folder32Bit;
			else throw new NotSupportedException("Unsupported bit size: " + IntPtr.Size.ToString());

			#if RELEASE
			const string config = "Release";
			#else
			const string config = "Debug";
			#endif

			// load api abstraction
			var abstractionDesc = new AbstractionDesc(true);
			abstractionDesc.deviceDescD3D12.window = window;
			abstractionDesc.nativeLibPathD3D12 = Path.Combine(platformPath, @"Shared\Orbital.Video.D3D12.Native\bin", libFolderBit, config);
			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out instance, out device)) throw new Exception("Failed to init abstraction");
			commandBuffer = device.CreateCommandBuffer();
		}

		public void Dispose()
		{
			if (commandBuffer != null)
			{
				commandBuffer.Dispose();
				commandBuffer = null;
			}

			if (device != null)
			{
				device.Dispose();
				device = null;
			}

			if (instance != null)
			{
				instance.Dispose();
				instance = null;
			}
		}

		public void Run()
		{
			while (!window.IsClosed())
			{
				application.RunEvents();

				device.BeginFrame();
				commandBuffer.Start();
				commandBuffer.EnabledRenderTarget();
				commandBuffer.ClearRenderTarget(1, 0, 0, 1);
				commandBuffer.EnabledPresent();
				commandBuffer.Finish();
				device.ExecuteCommandBuffer(commandBuffer);
				device.EndFrame();
			}
		}
	}
}
