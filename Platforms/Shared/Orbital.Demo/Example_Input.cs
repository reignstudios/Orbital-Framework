using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Orbital.Host;
using Orbital.Numerics;

using Orbital.Input;
using Orbital.Input.API;

namespace Orbital.Demo
{
	public sealed partial class Example
	{
		private InstanceBase inputInstance;

		private void InitInput(string platformPath, string libFolderBit, string config)
		{
			// load api abstraction (api-instance and hardware-device)
			var abstractionDesc = new AbstractionDesc(AbstractionInitType.SingleAPI);
			abstractionDesc.supportedAPIs = new AbstractionAPI[]
			{
				AbstractionAPI.XInput
			};

			#if DEBUG
			abstractionDesc.nativeLibPathDirectInput = Path.Combine(platformPath, @"Shared\Orbital.Input.DirectInput.Native\bin", libFolderBit, config);
			#else
			abstractionDesc.nativeLibPathDirectInput = string.Empty;
			#endif

			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out inputInstance)) throw new Exception("Failed to init input abstraction");
			inputInstance.DeviceConnectedCallback += Instance_DeviceConnectedCallback;
			inputInstance.GamepadConnectedCallback += Instance_GamepadConnectedCallback;

			if (inputInstance is Input.XInput.Instance)
			{
				var instanceXI = (Input.XInput.Instance)inputInstance;
				Log("XInput Version: " + instanceXI.version.ToString());
			}

			if (inputInstance is Input.DirectInput.Instance)
			{
				var instanceDI = (Input.DirectInput.Instance)inputInstance;
				Log("DirectInput FeatureLevel: " + instanceDI.featureLevel.ToString());
			}
		}

		private void DisposeInput()
		{
			if (inputInstance != null)
			{
				inputInstance.Dispose();
				inputInstance = null;
			}
		}

		private void UpdateInput()
		{
			inputInstance.Update();

			// print raw device input
			/*foreach (var device in inputInstance.devices)
			{
				if (!device.connected) continue;

				// buttons
				for (int i = 0; i != device.buttons.Length; ++i)
				{
					var button = device.buttons[i];
					if (button.down) Log("Button: " + i.ToString());
				}

				// POV
				for (int i = 0; i != device.povDirections.Length; ++i)
				{
					uint pov = device.povDirections[i];
					if (pov != uint.MaxValue) Log("POV: " + i.ToString() + " " + pov.ToString());
				}

				// axes 1D
				for (int i = 0; i != device.axes1D.Length; ++i)
				{
					var axis = device.axes1D[i];
					if (axis.value != 0) Log("Axis1D: " + i.ToString() + " " + axis.value.ToString());
				}

				// axes 2D
				for (int i = 0; i != device.axes2D.Length; ++i)
				{
					var axis = device.axes2D[i];
					if (axis.value.Length() > 0) Log("Axis2D: " + i.ToString() + " " + axis.value.ToString());
				}

				// axes 3D
				for (int i = 0; i != device.axes3D.Length; ++i)
				{
					var axis = device.axes3D[i];
					if (axis.value.Length() > 0) Log("Axis3D: " + i.ToString() + " " + axis.value.ToString());
				}

				// sliders
				for (int i = 0; i != device.sliders.Length; ++i)
				{
					var slider = device.sliders[i];
					if (slider.value != 0) Log("Slider: " + i.ToString() + " " + slider.value.ToString());
				}
			}*/

			// print gamepad input
			foreach (var gamepad in inputInstance.gamepads)
			{
				if (!gamepad.connected) continue;

				// rumble
				gamepad.SetRumble(gamepad.triggerLeft.value, gamepad.triggerRight.value);

				// buttons
				if (gamepad.button1.down) Log(gamepad.GetButtonName(gamepad.button1));
				if (gamepad.button2.down) Log(gamepad.GetButtonName(gamepad.button2));
				if (gamepad.button3.down) Log(gamepad.GetButtonName(gamepad.button3));
				if (gamepad.button4.down) Log(gamepad.GetButtonName(gamepad.button4));
				if (gamepad.button5.down) Log(gamepad.GetButtonName(gamepad.button5));
				if (gamepad.button6.down) Log(gamepad.GetButtonName(gamepad.button6));

				if (gamepad.special1.down) Log(gamepad.GetButtonName(gamepad.special1));
				if (gamepad.special2.down) Log(gamepad.GetButtonName(gamepad.special2));

				if (gamepad.dpadLeft.down) Log(gamepad.GetButtonName(gamepad.dpadLeft));
				if (gamepad.dpadRight.down) Log(gamepad.GetButtonName(gamepad.dpadRight));
				if (gamepad.dpadDown.down) Log(gamepad.GetButtonName(gamepad.dpadDown));
				if (gamepad.dpadUp.down) Log(gamepad.GetButtonName(gamepad.dpadUp));

				if (gamepad.home.down) Log(gamepad.GetButtonName(gamepad.home));
				if (gamepad.menu.down) Log(gamepad.GetButtonName(gamepad.menu));
				if (gamepad.back.down) Log(gamepad.GetButtonName(gamepad.back));

				if (gamepad.bumperLeft.down) Log(gamepad.GetButtonName(gamepad.bumperLeft));
				if (gamepad.bumperRight.down) Log(gamepad.GetButtonName(gamepad.bumperRight));

				if (gamepad.triggerButtonLeft.down) Log(gamepad.GetButtonName(gamepad.triggerButtonLeft));
				if (gamepad.triggerButtonRight.down) Log(gamepad.GetButtonName(gamepad.triggerButtonRight));

				if (gamepad.joystickButtonLeft.down) Log(gamepad.GetButtonName(gamepad.joystickButtonLeft));
				if (gamepad.joystickButtonRight.down) Log(gamepad.GetButtonName(gamepad.joystickButtonRight));

				// triggers
				if (gamepad.triggerLeft.value != 0) Log(gamepad.GetTriggerName(gamepad.triggerLeft) + " " + gamepad.triggerLeft.value.ToString());
				if (gamepad.triggerRight.value != 0) Log(gamepad.GetTriggerName(gamepad.triggerRight) + " " + gamepad.triggerRight.value.ToString());

				// joysticks
				if (gamepad.joystickLeft.value.Length() != 0) Log(gamepad.GetJoystickName(gamepad.joystickLeft) + " " + gamepad.joystickLeft.value.ToString());
				if (gamepad.joystickRight.value.Length() != 0) Log(gamepad.GetJoystickName(gamepad.joystickRight) + " " + gamepad.joystickRight.value.ToString());
			}
		}

		private void Instance_DeviceConnectedCallback(DeviceBase device)
		{
			Log("Device connected");
			device.DisconnectedCallback += Device_DisconnectedCallback;
		}

		private void Device_DisconnectedCallback(DeviceBase device)
		{
			Log("Device disconnected");
		}

		private void Instance_GamepadConnectedCallback(Gamepad gamepad)
		{
			Log("Gamepad connected");
		}
	}
}
