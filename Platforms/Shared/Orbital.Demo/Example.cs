using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.Host;
using Orbital.Video;
using Orbital.Video.D3D12;

namespace Orbital.Demo
{
	public sealed class Example : IDisposable
	{
		private ApplicationBase application;
		private WindowBase window;
		private IntPtr hWnd;

		private DeviceBase device;
		private CommandBufferBase commandBuffer;

		[DllImport("Kernel32.dll", EntryPoint = "LoadLibraryA")]
		private static extern unsafe IntPtr LoadLibraryA(byte* lpLibFileName);

		public Example(ApplicationBase application, WindowBase window, IntPtr hWnd)
		{
			this.application = application;
			this.window = window;
			this.hWnd = hWnd;
		}

		private unsafe void LoadLib(string libPath)
		{
			byte[] libNameEncoded = Encoding.Default.GetBytes(libPath);
			fixed (byte* libNameEncodedPtr = libNameEncoded)
			{
				IntPtr lib = LoadLibraryA(libNameEncodedPtr);
				if (lib == IntPtr.Zero) throw new Exception("Failed to load lib: " + libPath);
			}
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

			#if WIN32
			string libPath = Path.Combine(platformPath, @"Shared\Orbital.Video.D3D12.Native\bin", libFolderBit, config, "Orbital.Video.D3D12.Native.dll");
			LoadLib(libPath);
			#endif

			// load api abstraction
			var deviceD3D12 = new Device(DeviceType.Presentation);
			var size = window.GetSize(WindowSizeType.WorkingArea);
			if (!deviceD3D12.Init(-1, FeatureLevel.Level_11_0, false, hWnd, size.width, size.height, 2, false)) throw new Exception("Failed to init D3D12 Device");
			device = deviceD3D12;

			var commandBufferD3D12 = new CommandBuffer(deviceD3D12);
			if (!commandBufferD3D12.Init()) throw new Exception("Failed to init D3D12 CommandBuffer");
			commandBuffer = commandBufferD3D12;
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
