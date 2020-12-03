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
		: base(16, 2, 2)
		{
			this.index = index;

			// primary buttons
			button1.name = "A";
			button2.name = "B";
			button3.name = "X";
			button4.name = "Y";

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

		/*internal void Update(bool connected, ref XINPUT_STATE state)
		{
			Update(connected);
			if (!connected) return;

			var gamepad = state.Gamepad;

			// primary buttons
			button1.Update((gamepad.wButtons & 0x1000) != 0);
			button2.Update((gamepad.wButtons & 0x2000) != 0);
			button3.Update((gamepad.wButtons & 0x4000) != 0);
			button4.Update((gamepad.wButtons & 0x8000) != 0);

			// dpad
			dpadLeft.Update((gamepad.wButtons & 0x0004) != 0);
			dpadRight.Update((gamepad.wButtons & 0x0008) != 0);
			dpadDown.Update((gamepad.wButtons & 0x0002) != 0);
			dpadUp.Update((gamepad.wButtons & 0x0001) != 0);

			// options
			menu.Update((gamepad.wButtons & 0x0010) != 0);
			back.Update((gamepad.wButtons & 0x0020) != 0);

			// bumbers
			bumperLeft.Update((gamepad.wButtons & 0x0100) != 0);
			bumperRight.Update((gamepad.wButtons & 0x0200) != 0);

			// trigger buttons
			float triggerLeftValue = gamepad.bLeftTrigger / 255f;
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
			joystickRight.Update(new Vec2(gamepad.sThumbRX / (float)short.MaxValue, gamepad.sThumbRY / (float)short.MaxValue));

			// update arrays
			UpdateArraysToCommon();
		}*/

		public override void UpdateArraysToCommon()
		{
			// primary buttons
			buttons[0] = button1;
			buttons[1] = button2;
			buttons[2] = button3;
			buttons[3] = button4;

			// dpad
			buttons[4] = dpadLeft;
			buttons[5] = dpadRight;
			buttons[6] = dpadDown;
			buttons[7] = dpadUp;

			// options
			buttons[8] = menu;
			buttons[9] = back;

			// bumbers
			buttons[10] = bumperLeft;
			buttons[11] = bumperRight;

			// trigger buttons
			buttons[12] = triggerButtonLeft;
			buttons[13] = triggerButtonRight;

			// joystick buttons
			buttons[14] = joystickButtonLeft;
			buttons[15] = joystickButtonRight;

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
