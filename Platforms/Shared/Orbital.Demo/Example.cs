using System;
using System.Threading;
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
		private DeviceBase device;

		[DllImport("Kernel32.dll", EntryPoint = "LoadLibraryA")]
		private static extern unsafe IntPtr LoadLibraryA(byte* lpLibFileName);

		public Example(ApplicationBase application, WindowBase window)
		{
			this.application = application;
			this.window = window;
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
			if (!deviceD3D12.Init(-1, FeatureLevel.Level_11_0, false)) throw new Exception("Failed to init D3D12");
		}

		public void Dispose()
		{
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
				Thread.Sleep(1000 / 60);
			}
		}
	}
}
