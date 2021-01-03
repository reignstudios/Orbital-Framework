using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;
using WORD = System.UInt16;

namespace Orbital.Input.XInput
{
	public sealed class Device : DeviceBase
	{
		public Instance instanceXI { get; private set; }

		/// <summary>
		/// The device index
		/// </summary>
		public int index { get; private set; }

		[DllImport(Instance.lib_1_3, CallingConvention = Instance.callingConvention, EntryPoint = "XInputGetState")]
		private unsafe static extern DWORD XInputGetState_1_3(DWORD dwUserIndex, XINPUT_STATE* pState);

		[DllImport(Instance.lib_1_3, CallingConvention = Instance.callingConvention, EntryPoint = "XInputSetState")]
		private unsafe static extern DWORD XInputSetState_1_3(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);

		public Device(Instance instance, int index)
		: base(instance)
		{
			instanceXI = instance;
			this.index = index;
			type = DeviceType.Gamepad;

			CreatePhysicalObjects(16, 2, 2, 0, 0);
			int buttonIndex = 0;

			// primary buttons
			button1 = buttons[buttonIndex++];
			button2 = buttons[buttonIndex++];
			button3 = buttons[buttonIndex++];
			button4 = buttons[buttonIndex++];
			button1.name = "A";
			button2.name = "B";
			button3.name = "X";
			button4.name = "Y";

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

		public bool Init()
		{
			return true;
		}

		public override void Dispose()
		{
			// do nothing...
		}

		public unsafe override void Update()
		{
			// get device state
			XINPUT_STATE state;
			bool connected;
			switch (instanceXI.version)
			{
				case InstanceVersion.XInput_1_3: connected = XInputGetState_1_3((uint)index, &state) == 0; break;
				default: throw new NotImplementedException();
			}

			// validate is connected
			Update(connected);
			if (!connected) return;

			// grab gamepad state
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
		}

		public unsafe override void SetRumble(float value)
		{
			if (value > 1) value = 1;
			if (value < 0) value = 0;
			var desc = new XINPUT_VIBRATION()
			{
				wLeftMotorSpeed = (WORD)(WORD.MaxValue * value),
				wRightMotorSpeed = (WORD)(WORD.MaxValue * value)
			};
			switch (instanceXI.version)
			{
				case InstanceVersion.XInput_1_3: XInputSetState_1_3((DWORD)index, &desc); break;
				default: throw new NotImplementedException();
			}
		}

		public unsafe override void SetRumble(float leftValue, float rightValue)
		{
			if (leftValue > 1) leftValue = 1;
			if (leftValue < 0) leftValue = 0;
			if (rightValue > 1) rightValue = 1;
			if (rightValue < 0) rightValue = 0;
			var desc = new XINPUT_VIBRATION()
			{
				wLeftMotorSpeed = (WORD)(WORD.MaxValue * leftValue),
				wRightMotorSpeed = (WORD)(WORD.MaxValue * rightValue)
			};
			switch (instanceXI.version)
			{
				case InstanceVersion.XInput_1_3: XInputSetState_1_3((DWORD)index, &desc); break;
				default: throw new NotImplementedException();
			}
		}

		public unsafe override void SetRumble(float value, int motorIndex)
		{
			if (value > 1) value = 1;
			if (value < 0) value = 0;
			var desc = new XINPUT_VIBRATION();
			if (motorIndex == 0) desc.wLeftMotorSpeed = (WORD)(WORD.MaxValue * value);
			if (motorIndex == 1) desc.wRightMotorSpeed = (WORD)(WORD.MaxValue * value);
			switch (instanceXI.version)
			{
				case InstanceVersion.XInput_1_3: XInputSetState_1_3((DWORD)index, &desc); break;
				default: throw new NotImplementedException();
			}
		}
	}
}
