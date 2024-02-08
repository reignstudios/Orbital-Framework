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
		private WindowBase window;

		private InstanceBase instance;

		public Example(WindowBase window)
		{
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
			abstractionDesc.supportedAPIs = new AbstractionAPI[] {AbstractionAPI.XInput};

			#if DEBUG
			abstractionDesc.nativeLibPathDirectInput = Path.Combine(platformPath, @"Shared\Orbital.Input.DirectInput.Native\bin", libFolderBit, config);
			#else
			abstractionDesc.nativeLibPathDirectInput = string.Empty;
			#endif

			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out instance)) throw new Exception("Failed to init abstraction");
			instance.DeviceConnectedCallback += Instance_DeviceConnectedCallback;
			instance.GamepadConnectedCallback += Instance_GamepadConnectedCallback;

			if (instance is Orbital.Input.XInput.Instance)
			{
				var instanceXI = (Orbital.Input.XInput.Instance)instance;
				Console.WriteLine("Version: " + instanceXI.version.ToString());
			}

			if (instance is Orbital.Input.DirectInput.Instance)
			{
				var instanceDI = (Orbital.Input.DirectInput.Instance)instance;
				Console.WriteLine("FeatureLevel: " + instanceDI.featureLevel.ToString());
			}
		}

		private void Instance_DeviceConnectedCallback(DeviceBase device)
		{
			Console.WriteLine("Device connected");
			device.DisconnectedCallback += Device_DisconnectedCallback;
		}

		private void Device_DisconnectedCallback(DeviceBase device)
		{
			Console.WriteLine("Device disconnected");
		}

		private void Instance_GamepadConnectedCallback(Gamepad gamepad)
		{
			Console.WriteLine("Gamepad connected");
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
			instance.Update();

			// print raw device input
			/*foreach (var device in instance.devices)
			{
				if (!device.connected) continue;

				// buttons
				for (int i = 0; i != device.buttons.Length; ++i)
				{
					var button = device.buttons[i];
					if (button.down) Console.WriteLine("Button: " + i.ToString());
				}

				// POV
				for (int i = 0; i != device.povDirections.Length; ++i)
				{
					uint pov = device.povDirections[i];
					if (pov != uint.MaxValue) Console.WriteLine("POV: " + i.ToString() + " " + pov.ToString());
				}

				// axes 1D
				for (int i = 0; i != device.axes1D.Length; ++i)
				{
					var axis = device.axes1D[i];
					if (axis.value != 0) Console.WriteLine("Axis1D: " + i.ToString() + " " + axis.value.ToString());
				}

				// axes 2D
				for (int i = 0; i != device.axes2D.Length; ++i)
				{
					var axis = device.axes2D[i];
					if (axis.value.Length() > 0) Console.WriteLine("Axis2D: " + i.ToString() + " " + axis.value.ToString());
				}

				// axes 3D
				for (int i = 0; i != device.axes3D.Length; ++i)
				{
					var axis = device.axes3D[i];
					if (axis.value.Length() > 0) Console.WriteLine("Axis3D: " + i.ToString() + " " + axis.value.ToString());
				}

				// sliders
				for (int i = 0; i != device.sliders.Length; ++i)
				{
					var slider = device.sliders[i];
					if (slider.value != 0) Console.WriteLine("Slider: " + i.ToString() + " " + slider.value.ToString());
				}
			}*/

			// print gamepad input
			foreach (var gamepad in instance.gamepads)
			{
				if (!gamepad.connected) continue;

				// rumble
				gamepad.SetRumble(gamepad.triggerLeft.value, gamepad.triggerRight.value);

				// buttons
				if (gamepad.button1.down) Console.WriteLine(gamepad.GetButtonName(gamepad.button1));
				if (gamepad.button2.down) Console.WriteLine(gamepad.GetButtonName(gamepad.button2));
				if (gamepad.button3.down) Console.WriteLine(gamepad.GetButtonName(gamepad.button3));
				if (gamepad.button4.down) Console.WriteLine(gamepad.GetButtonName(gamepad.button4));
				if (gamepad.button5.down) Console.WriteLine(gamepad.GetButtonName(gamepad.button5));
				if (gamepad.button6.down) Console.WriteLine(gamepad.GetButtonName(gamepad.button6));

				if (gamepad.special1.down) Console.WriteLine(gamepad.GetButtonName(gamepad.special1));
				if (gamepad.special2.down) Console.WriteLine(gamepad.GetButtonName(gamepad.special2));

				if (gamepad.dpadLeft.down) Console.WriteLine(gamepad.GetButtonName(gamepad.dpadLeft));
				if (gamepad.dpadRight.down) Console.WriteLine(gamepad.GetButtonName(gamepad.dpadRight));
				if (gamepad.dpadDown.down) Console.WriteLine(gamepad.GetButtonName(gamepad.dpadDown));
				if (gamepad.dpadUp.down) Console.WriteLine(gamepad.GetButtonName(gamepad.dpadUp));

				if (gamepad.home.down) Console.WriteLine(gamepad.GetButtonName(gamepad.home));
				if (gamepad.menu.down) Console.WriteLine(gamepad.GetButtonName(gamepad.menu));
				if (gamepad.back.down) Console.WriteLine(gamepad.GetButtonName(gamepad.back));

				if (gamepad.bumperLeft.down) Console.WriteLine(gamepad.GetButtonName(gamepad.bumperLeft));
				if (gamepad.bumperRight.down) Console.WriteLine(gamepad.GetButtonName(gamepad.bumperRight));

				if (gamepad.triggerButtonLeft.down) Console.WriteLine(gamepad.GetButtonName(gamepad.triggerButtonLeft));
				if (gamepad.triggerButtonRight.down) Console.WriteLine(gamepad.GetButtonName(gamepad.triggerButtonRight));

				if (gamepad.joystickButtonLeft.down) Console.WriteLine(gamepad.GetButtonName(gamepad.joystickButtonLeft));
				if (gamepad.joystickButtonRight.down) Console.WriteLine(gamepad.GetButtonName(gamepad.joystickButtonRight));

				// triggers
				if (gamepad.triggerLeft.value != 0) Console.WriteLine(gamepad.GetTriggerName(gamepad.triggerLeft) + " " + gamepad.triggerLeft.value.ToString());
				if (gamepad.triggerRight.value != 0) Console.WriteLine(gamepad.GetTriggerName(gamepad.triggerRight) + " " + gamepad.triggerRight.value.ToString());

				// joysticks
				if (gamepad.joystickLeft.value.Length() != 0) Console.WriteLine(gamepad.GetJoystickName(gamepad.joystickLeft) + " " + gamepad.joystickLeft.value.ToString());
				if (gamepad.joystickRight.value.Length() != 0) Console.WriteLine(gamepad.GetJoystickName(gamepad.joystickRight) + " " + gamepad.joystickRight.value.ToString());
			}

			// keep within 60fps
			System.Threading.Thread.Sleep(1000 / 60);
		}
	}
}
