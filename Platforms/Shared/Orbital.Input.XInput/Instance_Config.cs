namespace Orbital.Input.XInput
{
	public sealed partial class Instance : InstanceBase
	{
		public override GamepadHardwareConfiguration[] GetGamepadHardwareConfigurations()
		{
			var hardwareConfig = new GamepadHardwareConfiguration();
			hardwareConfig.config = GamepadConfiguration_Micorsoft_Xbox();

			var hardwareConfigurations = new GamepadHardwareConfiguration[1];
			hardwareConfigurations[0] = hardwareConfig;
			return hardwareConfigurations;
		}

		private GamepadConfiguration GamepadConfiguration_Micorsoft_Xbox()
		{
			var config = new GamepadConfiguration();

			// primary buttons
			config.button1.index = 0;
			config.button2.index = 1;
			config.button3.index = 2;
			config.button4.index = 3;
			config.button1.name = "A";
			config.button2.name = "B";
			config.button3.name = "X";
			config.button4.name = "Y";

			// dpad
			config.dpadLeft.index = 4;
			config.dpadRight.index = 5;
			config.dpadDown.index = 6;
			config.dpadUp.index = 7;
			config.dpadLeft.name = "Left";
			config.dpadRight.name = "Right";
			config.dpadDown.name = "Down";
			config.dpadUp.name = "Up";

			// options
			config.menu.index = 8;
			config.back.index = 9;
			config.menu.name = "Menu";
			config.back.name = "Back";

			// bumbers
			config.bumperLeft.index = 10;
			config.bumperRight.index = 11;
			config.bumperLeft.name = "BL";
			config.bumperRight.name = "BR";

			// trigger buttons
			config.triggerButtonLeft.index = 12;
			config.triggerButtonRight.index = 13;
			config.triggerButtonLeft.name = "TBL";
			config.triggerButtonRight.name = "TBR";

			// joystick buttons
			config.joystickButtonLeft.index = 14;
			config.joystickButtonRight.index = 15;
			config.joystickButtonLeft.name = "JBL";
			config.joystickButtonRight.name = "JBR";

			// triggers
			config.triggerLeft_Axis1D.index = 0;
			config.triggerRight_Axis1D.index = 1;
			config.triggerLeft_Axis1D.name = "TL";
			config.triggerRight_Axis1D.name = "TR";

			// triggers
			config.joystickLeft_Axis2D.index = 0;
			config.joystickRight_Axis2D.index = 1;
			config.joystickLeft_Axis2D.name = "JL";
			config.joystickRight_Axis2D.name = "JR";

			return config;
		}
	}
}
