using System;
using System.Diagnostics;
using System.IO;

using Orbital.Host;
using Orbital.Numerics;
using Orbital.Input;
using Orbital.Input.API;

namespace Orbital.Demo
{
	public sealed class Example : IDisposable
	{
		private ApplicationBase application;
		private WindowBase window;

		private InstanceBase instance;

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

			// load api abstraction (api-instance and hardware-device)
			var abstractionDesc = new AbstractionDesc(AbstractionInitType.SingleAPI);
			abstractionDesc.supportedAPIs = new AbstractionAPI[] {AbstractionAPI.DirectInput};

			#if DEBUG
			abstractionDesc.nativeLibPathDirectInput = Path.Combine(platformPath, @"Shared\Orbital.Input.DirectInput.Native\bin", libFolderBit, config);
			#else
			abstractionDesc.nativeLibPathDirectInput = string.Empty;
			#endif

			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out instance)) throw new Exception("Failed to init abstraction");
		}

		public void Dispose()
		{
			if (instance != null)
			{
				instance.Dispose();
				instance = null;
			}
		}
		
		public void Run()
		{
			application.RunEvents();// run this once before checking window
			while (!window.IsClosed())
			{
				instance.Update();
				foreach (var device in instance.GetDevices())
				{
					if (!device.connected) continue;

					// rumble
					device.SetRumble(device.triggerLeft.value, device.triggerRight.value);

					// buttons
					for (int i = 0; i != device.buttons.Length; ++i)
					{
						var button = device.buttons[i];
						if (button.down) Console.WriteLine(i.ToString() + " " + button.name);
					}

					// analogs 1D
					for (int i = 0; i != device.analogs_1D.Length; ++i)
					{
						var analog = device.analogs_1D[i];
						if (analog.value != 0) Console.WriteLine(i.ToString() + " " + analog.name + " " + analog.value.ToString());
					}

					// analogs 2D
					for (int i = 0; i != device.analogs_2D.Length; ++i)
					{
						var analog = device.analogs_2D[i];
						if (analog.value.Length() > 0) Console.WriteLine(i.ToString() + " " + analog.name + " " + analog.value.ToString());
					}
				}

				// keep within 60fps
				System.Threading.Thread.Sleep(16);

				// run application events
				application.RunEvents();
			}
		}
	}
}
