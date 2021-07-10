using Orbital.Numerics;

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

		public Device(Instance instance, int index)
		: base(instance)
		{
			instanceXI = instance;
			this.index = index;
			type = DeviceType.Gamepad;
			CreatePhysicalObjects(16, 2, 2, 0, 0, 0);
		}

		public unsafe override void Update()
		{
			// get device state
			XINPUT_STATE state;
			bool connected = instanceXI.XInputGetState((uint)index, &state) == 0;

			// validate is connected
			UpdateStart(connected);
			if (!connected) return;

			// grab gamepad state
			var gamepad = state.Gamepad;

			// primary buttons
			buttons[0].Update((gamepad.wButtons & 0x1000) != 0);// 1
			buttons[1].Update((gamepad.wButtons & 0x2000) != 0);// 2
			buttons[2].Update((gamepad.wButtons & 0x4000) != 0);// 3
			buttons[3].Update((gamepad.wButtons & 0x8000) != 0);// 4

			// dpad
			buttons[4].Update((gamepad.wButtons & 0x0004) != 0);// left
			buttons[5].Update((gamepad.wButtons & 0x0008) != 0);// right
			buttons[6].Update((gamepad.wButtons & 0x0002) != 0);// down
			buttons[7].Update((gamepad.wButtons & 0x0001) != 0);// up

			// options
			buttons[8].Update((gamepad.wButtons & 0x0010) != 0);// menu
			buttons[9].Update((gamepad.wButtons & 0x0020) != 0);// back

			// bumbers
			buttons[10].Update((gamepad.wButtons & 0x0100) != 0);// bumper left
			buttons[11].Update((gamepad.wButtons & 0x0200) != 0);// bumper right

			// trigger buttons
			float triggerLeftValue = gamepad.bLeftTrigger / 255f;
			float triggerRightValue = gamepad.bRightTrigger / 255f;
			buttons[12].Update(triggerLeftValue >= .75f);// trigger button left
			buttons[13].Update(triggerRightValue >= .75f);// trigger button right

			// joystick buttons
			buttons[14].Update((gamepad.wButtons & 0x0040) != 0);// joystick button left
			buttons[15].Update((gamepad.wButtons & 0x0080) != 0);// joystick button right

			// triggers
			axes1D[0].Update(triggerLeftValue);// trigger left
			axes1D[1].Update(triggerRightValue);// trigger right

			// joysticks
			axes2D[0].Update(new Vec2(gamepad.sThumbLX / (float)short.MaxValue, gamepad.sThumbLY / (float)short.MaxValue));// joystick left
			axes2D[1].Update(new Vec2(gamepad.sThumbRX / (float)short.MaxValue, gamepad.sThumbRY / (float)short.MaxValue));// joystick right
		}

		protected override void RefreshDeviceInfo()
		{
			// Do nothing: XInput doesn't provide device info by design and all configurations are gamepads
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
			instanceXI.XInputSetState((DWORD)index, &desc);
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
			instanceXI.XInputSetState((DWORD)index, &desc);
		}

		public unsafe override void SetRumble(float value, int motorIndex)
		{
			if (value > 1) value = 1;
			if (value < 0) value = 0;
			var desc = new XINPUT_VIBRATION();
			if (motorIndex == 0) desc.wLeftMotorSpeed = (WORD)(WORD.MaxValue * value);
			if (motorIndex == 1) desc.wRightMotorSpeed = (WORD)(WORD.MaxValue * value);
			instanceXI.XInputSetState((DWORD)index, &desc);
		}
	}
}
