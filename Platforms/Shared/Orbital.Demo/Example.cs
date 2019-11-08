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

		private DeviceBase device;
		private CommandBufferBase commandBuffer;

		public Example(ApplicationBase application, WindowBase window)
		{
			this.application = application;
			this.window = window;
		}

		public unsafe void Init(string platformPath)
		{
			// pre-load native libs
			string libFolderBit;
			if (IntPtr.Size == 8) libFolderBit = "x64";
			else if (IntPtr.Size == 4) libFolderBit = "x86";
			else throw new NotSupportedException("Unsupported bit size: " + IntPtr.Size.ToString());

			#if RELEASE
			const string config = "Release";
			#else
			const string config = "Debug";
			#endif

			// load api abstraction
			var deviceDesc = new DeviceDesc();
			deviceDesc.descD3D12.window = window;
			deviceDesc.nativeLibPathD3D12 = Path.Combine(platformPath, @"Shared\Orbital.Video.D3D12.Native\bin", libFolderBit, config);
			device = Device.Init(deviceDesc);
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
