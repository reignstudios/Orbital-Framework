using System;
using System.Runtime.InteropServices;

namespace Orbital.Input.DirectInput
{
	public sealed class Device : DeviceBase
	{
		public Instance instanceDI { get; private set; }

		/// <summary>
		/// Primary device configured in Windows control panel
		/// </summary>
		public bool isPrimary { get; private set; }

		/// <summary>
		/// The device index
		/// </summary>
		public int index { get; private set; }

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern int Orbital_Video_DirectInput_Instance_GetDeviceState(IntPtr handle, int deviceIndex, DIJOYSTATE2* state, int* connected);

		public Device(Instance instance, int index)
		: base(instance)
		{
			instanceDI = instance;
			this.index = index;

			CreateAttachedArrays(18, 2, 2);
			int buttonIndex = 0;

			// primary buttons
			button1 = buttons[buttonIndex++];
			button2 = buttons[buttonIndex++];
			button3 = buttons[buttonIndex++];
			button4 = buttons[buttonIndex++];
			button5 = buttons[buttonIndex++];
			button6 = buttons[buttonIndex++];
			button1.name = "A";
			button2.name = "B";
			button3.name = "X";
			button4.name = "Y";
			button5.name = "C";
			button6.name = "Z";

			// dpad
			dpadLeft = buttons[buttonIndex++];
			dpadRight = buttons[buttonIndex++];
			dpadDown = buttons[buttonIndex++];
			dpadUp = buttons[buttonIndex++];
			dpadLeft.name = "Left";
			dpadRight.name = "Right";
			dpadDown.name = "Down";
			dpadUp.name = "Up";

			// options
			menu = buttons[buttonIndex++];
			back = buttons[buttonIndex++];
			menu.name = "Menu";
			back.name = "Back";

			// bumbers
			bumperLeft = buttons[buttonIndex++];
			bumperRight = buttons[buttonIndex++];
			bumperLeft.name = "BL";
			bumperRight.name = "BR";

			// trigger buttons
			triggerButtonLeft = buttons[buttonIndex++];
			triggerButtonRight = buttons[buttonIndex++];
			triggerButtonLeft.name = "TBL";
			triggerButtonRight.name = "TBR";

			// joystick buttons
			joystickButtonLeft = buttons[buttonIndex++];
			joystickButtonRight = buttons[buttonIndex++];
			joystickButtonLeft.name = "JBL";
			joystickButtonRight.name = "JBR";

			// triggers
			triggerLeft = analogs_1D[0];
			triggerRight = analogs_1D[1];
			triggerLeft.name = "TL";
			triggerRight.name = "TR";

			// triggers
			joystickLeft = analogs_2D[0];
			joystickRight = analogs_2D[1];
			joystickLeft.name = "JL";
			joystickRight.name = "JR";

			// create any missing objects this API doesn't support
			CreateMissingObjects();
		}

		public void Init(bool isPrimary)
		{
			this.isPrimary = isPrimary;
		}

		public override void Dispose()
		{
			// do nothing...
		}

		public unsafe override void Update()
		{
			// get device state
			DIJOYSTATE2 state;
			int connectedValue;
			bool connected = Orbital_Video_DirectInput_Instance_GetDeviceState(instanceDI.handle, index, &state, &connectedValue) != 0 && connectedValue != 0;

			// validate is connected
			Update(connected);
			if (!connected) return;

			//for (int i = 0; i != 128; ++i)
			//{
			//	if (state.rgbButtons[i] != 0)
			//	{
			//		System.Console.WriteLine($"{i} = {state.rgbButtons[i]}");
			//	}
			//}

			// primary buttons
			button1.Update(state.rgbButtons[0] != 0);
			button2.Update(state.rgbButtons[1] != 0);
			button3.Update(state.rgbButtons[2] != 0);
			button4.Update(state.rgbButtons[3] != 0);

			// dpad
			dpadLeft.Update(state.rgbButtons[4] != 0);
			dpadRight.Update(state.rgbButtons[5] != 0);
			dpadDown.Update(state.rgbButtons[6] != 0);
			dpadUp.Update(state.rgbButtons[7] != 0);

			// options
			menu.Update(state.rgbButtons[8] != 0);
			back.Update(state.rgbButtons[9] != 0);

			// bumbers
			bumperLeft.Update(state.rgbButtons[10] != 0);
			bumperRight.Update(state.rgbButtons[11] != 0);

			/*// trigger buttons
			float triggerLeftValue = state.lY / 255f;
			float triggerRightValue = gamepad.bRightTrigger / 255f;
			triggerButtonLeft.Update(triggerLeftValue >= .75f);
			triggerButtonRight.Update(triggerRightValue >= .75f);

			// joystick buttons
			joystickButtonLeft.Update((gamepad.wButtons & 0x0040) != 0);
			joystickButtonRight.Update((gamepad.wButtons & 0x0080) != 0);

			// triggers
			triggerLeft.Update(triggerLeftValue);
			triggerRight.Update(triggerRightValue);

			// joysticks
			joystickLeft.Update(new Vec2(gamepad.sThumbLX / (float)short.MaxValue, gamepad.sThumbLY / (float)short.MaxValue));
			joystickRight.Update(new Vec2(gamepad.sThumbRX / (float)short.MaxValue, gamepad.sThumbRY / (float)short.MaxValue));*/
		}
	}
}
