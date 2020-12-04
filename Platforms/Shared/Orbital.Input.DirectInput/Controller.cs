using Orbital.Numerics;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;
using WORD = System.UInt16;

namespace Orbital.Input.DirectInput
{
	public sealed class Controller : ControllerBase
	{
		public int index { get; private set; }

		//[DllImport(Instance.lib_8, CallingConvention = Instance.callingConvention, EntryPoint = "XInputSetState")]
		//private unsafe static extern DWORD XInputSetState_1_3(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);

		public Controller(int index)
		: base(18, 2, 2)
		{
			this.index = index;

			// primary buttons
			button1.name = "A";
			button2.name = "B";
			button3.name = "X";
			button4.name = "Y";
			button5.name = "C";
			button6.name = "Z";

			// dpad
			dpadLeft.name = "Left";
			dpadRight.name = "Right";
			dpadDown.name = "Down";
			dpadUp.name = "Up";

			// options
			menu.name = "Menu";
			back.name = "Back";

			// bumbers
			bumperLeft.name = "BL";
			bumperRight.name = "BR";

			// trigger buttons
			triggerButtonLeft.name = "TBL";
			triggerButtonRight.name = "TBR";

			// joystick buttons
			joystickButtonLeft.name = "JBL";
			joystickButtonRight.name = "JBR";

			// triggers
			triggerLeft.name = "TL";
			triggerRight.name = "TR";

			// triggers
			joystickLeft.name = "JL";
			joystickRight.name = "JR";
		}

		internal unsafe void Update(bool connected, ref DIJOYSTATE2 state)
		{
			Update(connected);
			if (!connected) return;

			for (int i = 0; i != 128; ++i)
			{
				if (state.rgbButtons[i] != 0)
				{
					System.Console.WriteLine($"{i} = {state.rgbButtons[i]}");
				}
			}

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

			// update arrays
			UpdateArraysToCommon();
		}

		public override void UpdateArraysToCommon()
		{
			int i = 0;

			// primary buttons
			buttons[i++] = button1;
			buttons[i++] = button2;
			buttons[i++] = button3;
			buttons[i++] = button4;
			buttons[i++] = button5;
			buttons[i++] = button6;

			// dpad
			buttons[i++] = dpadLeft;
			buttons[i++] = dpadRight;
			buttons[i++] = dpadDown;
			buttons[i++] = dpadUp;

			// options
			buttons[i++] = menu;
			buttons[i++] = back;

			// bumbers
			buttons[i++] = bumperLeft;
			buttons[i++] = bumperRight;

			// trigger buttons
			buttons[i++] = triggerButtonLeft;
			buttons[i++] = triggerButtonRight;

			// joystick buttons
			buttons[i++] = joystickButtonLeft;
			buttons[i++] = joystickButtonRight;

			// triggers
			analogs_1D[0] = triggerLeft;
			analogs_1D[1] = triggerRight;

			// joysticks
			analogs_2D[0] = joystickLeft;
			analogs_2D[1] = joystickRight;
		}

		public unsafe override void SetRumble(float value)
		{
			//if (value > 1) value = 1;
			//if (value < 0) value = 0;
			//var desc = new XINPUT_VIBRATION()
			//{
			//	wLeftMotorSpeed = (WORD)(WORD.MaxValue * value),
			//	wRightMotorSpeed = (WORD)(WORD.MaxValue * value)
			//};
			//XInputSetState_1_3((DWORD)index, &desc);
		}

		public unsafe override void SetRumble(float leftValue, float rightValue)
		{
			//if (leftValue > 1) leftValue = 1;
			//if (leftValue < 0) leftValue = 0;
			//if (rightValue > 1) rightValue = 1;
			//if (rightValue < 0) rightValue = 0;
			//var desc = new XINPUT_VIBRATION()
			//{
			//	wLeftMotorSpeed = (WORD)(WORD.MaxValue * leftValue),
			//	wRightMotorSpeed = (WORD)(WORD.MaxValue * rightValue)
			//};
			//XInputSetState_1_3((DWORD)index, &desc);
		}
	}
}
