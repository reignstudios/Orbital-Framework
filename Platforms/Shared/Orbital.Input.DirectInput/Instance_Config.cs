namespace Orbital.Input.DirectInput
{
	public sealed partial class Instance : InstanceBase
	{
		public override GamepadHardwareConfiguration[] GetGamepadHardwareConfigurations()
		{
			var hardwareConfigurations = new GamepadHardwareConfiguration[1];
			int c = 0;

			// ================
			// Microsoft
			// ================
			//var xbox360ID = Guid.Parse("028e045e-0000-0000-0000-504944564944");
			//var logitechXInputID = Guid.Parse("c21d046d-0000-0000-0000-504944564944");// Logitech XInput mode
			//var logitechDirecInputID = Guid.Parse("c216046d-0000-0000-0000-504944564944");// Logitech DirectInput mode
			//var pxnArcadeStickID = Guid.Parse("187c0079-0000-0000-0000-504944564944");// PXN arcade stick
			/*bool isXbox360 = productID_GUID == xbox360ID || productID_GUID == logitechXInputID || productID_GUID == pxnArcadeStickID;
			if
			(
				isXbox360 ||// Xbox 360
				productID_GUID == Guid.Parse("02ff045e-0000-0000-0000-504944564944")// Xbox One
			)*/
			{
				// Xbox One
				var hardwareConfig = new GamepadHardwareConfiguration();
				hardwareConfig.productID = 0x02FF;//HID\VID_045E&PID_02FF&IG_00
				hardwareConfig.config = GamepadConfiguration_Micorsoft_Xbox(false);
				hardwareConfigurations[c++] = hardwareConfig;
			}
			/*else if (productID_GUID == logitechDirecInputID)// Logitech DirectInput mode
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = DeviceTriggerButtonMode.Physical;

				// primary buttons
				configuration.button1 = buttons[1];
				configuration.button2 = buttons[2];
				configuration.button3 = buttons[0];
				configuration.button4 = buttons[3];
				configuration.button1.name = "A";
				configuration.button2.name = "B";
				configuration.button3.name = "X";
				configuration.button4.name = "Y";

				// dpad
				configuration.dpadLeft = new Button(true);
				configuration.dpadRight = new Button(true);
				configuration.dpadDown = new Button(true);
				configuration.dpadUp = new Button(true);
				configuration.dpadLeft.name = "Left";
				configuration.dpadRight.name = "Right";
				configuration.dpadDown.name = "Down";
				configuration.dpadUp.name = "Up";

				// options
				configuration.menu = buttons[9];
				configuration.back = buttons[8];
				configuration.menu.name = "Start";
				configuration.back.name = "Back";

				// bumbers
				configuration.bumperLeft = buttons[4];
				configuration.bumperRight = buttons[5];
				configuration.bumperLeft.name = "BL";
				configuration.bumperRight.name = "BR";

				// trigger buttons
				configuration.triggerButtonLeft = buttons[6];
				configuration.triggerButtonRight = buttons[7];
				configuration.triggerButtonLeft.name = "TBL";
				configuration.triggerButtonRight.name = "TBR";

				// joystick buttons
				configuration.joystickButtonLeft = buttons[10];
				configuration.joystickButtonRight = buttons[11];
				configuration.joystickButtonLeft.name = "JBL";
				configuration.joystickButtonRight.name = "JBR";

				// joysticks
				configuration.joystickLeft = new Axis2D(true);
				configuration.joystickRight = new Axis2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX_Src = axes1D[0];
				configuration.axis2DMaps[0].axisY_Src = axes1D[1];
				configuration.axis2DMaps[0].axisDst = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX_Src = axes1D[2];
				configuration.axis2DMaps[1].axisY_Src = axes1D[3];
				configuration.axis2DMaps[1].axisDst = configuration.joystickRight;
			}

			// ================
			// Sony
			// ================
			var ps3ID_Wireless = Guid.Parse("0268054c-0000-0000-0000-504944564944");
			var ps3ID_Wired = Guid.Parse("63020e6f-0000-0000-0000-504944564944");
			var ps5ID = Guid.Parse("0ce6054c-0000-0000-0000-504944564944");
			if
			(
				productID_GUID == Guid.Parse("05c4054c-0000-0000-0000-504944564944") ||// PS4
				productID_GUID == ps5ID ||// PS5
				productID_GUID == ps3ID_Wired// Wired PS3 controller
			)
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = productID_GUID == ps3ID_Wired ? DeviceTriggerButtonMode.Physical : DeviceTriggerButtonMode.Virtual;

				// primary buttons
				configuration.button1 = buttons[1];
				configuration.button2 = buttons[2];
				configuration.button3 = buttons[0];
				configuration.button4 = buttons[3];
				configuration.button1.name = "X";
				configuration.button2.name = "O";
				configuration.button3.name = "Square";
				configuration.button4.name = "Triangle";

				// special button
				if (productID_GUID != ps3ID_Wired)
				{
					configuration.special1 = buttons[13];
					configuration.special1.name = "Touch-Pad";
				}

				if (productID_GUID == ps5ID)
				{
					configuration.special2 = buttons[14];
					configuration.special2.name = "Mute";
				}

				// dpad
				configuration.dpadLeft = new Button(true);
				configuration.dpadRight = new Button(true);
				configuration.dpadDown = new Button(true);
				configuration.dpadUp = new Button(true);
				configuration.dpadLeft.name = "Left";
				configuration.dpadRight.name = "Right";
				configuration.dpadDown.name = "Down";
				configuration.dpadUp.name = "Up";

				// options
				configuration.menu = buttons[9];
				configuration.back = buttons[8];
				if (productID_GUID == ps3ID_Wired)
				{
					configuration.menu.name = "Start";
					configuration.back.name = "Select";
				}
				else
				{
					configuration.home = buttons[12];
					configuration.menu.name = "Options";
					configuration.back.name = "Share";
					configuration.home.name = "PS";
				}

				// bumbers
				configuration.bumperLeft = buttons[4];
				configuration.bumperRight = buttons[5];
				configuration.bumperLeft.name = "BL";
				configuration.bumperRight.name = "BR";

				// trigger buttons
				if (productID_GUID == ps3ID_Wired)
				{
					configuration.triggerButtonLeft = buttons[6];
					configuration.triggerButtonRight = buttons[7];
				}
				else
				{
					configuration.triggerButtonLeft = new Button(false);
					configuration.triggerButtonRight = new Button(false);
				}
				configuration.triggerButtonLeft.name = "TBL";
				configuration.triggerButtonRight.name = "TBR";

				// joystick buttons
				configuration.joystickButtonLeft = buttons[10];
				configuration.joystickButtonRight = buttons[11];
				configuration.joystickButtonLeft.name = "JBL";
				configuration.joystickButtonRight.name = "JBR";

				// triggers
				if (productID_GUID != ps3ID_Wired)
				{
					configuration.triggerLeft = new Axis1D(true, Axis1DUpdateMode.FullRange_ShiftedPositive);
					configuration.triggerRight = new Axis1D(true, Axis1DUpdateMode.FullRange_ShiftedPositive);
					configuration.triggerLeft.name = "TL";
					configuration.triggerRight.name = "TR";

					configuration.axis1DMaps = new DeviceAxis1DMap[2];
					configuration.axis1DMaps[0].axisSrc = axes1D[3];
					configuration.axis1DMaps[0].axisDst = configuration.triggerLeft;
					configuration.axis1DMaps[1].axisSrc = axes1D[4];
					configuration.axis1DMaps[1].axisDst = configuration.triggerRight;
				}

				// joysticks
				configuration.joystickLeft = new Axis2D(true);
				configuration.joystickRight = new Axis2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX_Src = axes1D[0];
				configuration.axis2DMaps[0].axisY_Src = axes1D[1];
				configuration.axis2DMaps[0].axisDst = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX_Src = axes1D[2];
				if (productID_GUID == ps3ID_Wired) configuration.axis2DMaps[1].axisY_Src = axes1D[3];
				else configuration.axis2DMaps[1].axisY_Src = axes1D[5];
				configuration.axis2DMaps[1].axisDst = configuration.joystickRight;
			}
			else if (productID_GUID == ps3ID_Wireless)
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.Buttons;
				configuration.triggerButtonMode = DeviceTriggerButtonMode.Physical;

				// primary buttons
				configuration.button1 = buttons[14];
				configuration.button2 = buttons[13];
				configuration.button3 = buttons[15];
				configuration.button4 = buttons[12];
				configuration.button1.name = "X";
				configuration.button2.name = "O";
				configuration.button3.name = "Square";
				configuration.button4.name = "Triangle";

				// dpad
				configuration.dpadLeft = buttons[7];
				configuration.dpadRight = buttons[5];
				configuration.dpadDown = buttons[6];
				configuration.dpadUp = buttons[4];
				configuration.dpadLeft.name = "Left";
				configuration.dpadRight.name = "Right";
				configuration.dpadDown.name = "Down";
				configuration.dpadUp.name = "Up";

				// options
				configuration.menu = buttons[3];
				configuration.back = buttons[0];
				configuration.home = buttons[16];
				configuration.menu.name = "Start";
				configuration.back.name = "Select";
				configuration.home.name = "PS";

				// bumbers
				configuration.bumperLeft = buttons[10];
				configuration.bumperRight = buttons[11];
				configuration.bumperLeft.name = "BL";
				configuration.bumperRight.name = "BR";

				// trigger buttons
				configuration.triggerButtonLeft = buttons[8];
				configuration.triggerButtonRight = buttons[9];
				configuration.triggerButtonLeft.name = "TBL";
				configuration.triggerButtonRight.name = "TBR";

				// joystick buttons
				configuration.joystickButtonLeft = buttons[1];
				configuration.joystickButtonRight = buttons[2];
				configuration.joystickButtonLeft.name = "JBL";
				configuration.joystickButtonRight.name = "JBR";

				// joysticks
				configuration.joystickLeft = new Axis2D(true);
				configuration.joystickRight = new Axis2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX_Src = axes1D[0];
				configuration.axis2DMaps[0].axisY_Src = axes1D[1];
				configuration.axis2DMaps[0].axisDst = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX_Src = axes1D[2];
				configuration.axis2DMaps[1].axisY_Src = axes1D[3];
				configuration.axis2DMaps[1].axisDst = configuration.joystickRight;
			}

			// ================
			// Nintendo
			// ================
			var smashControllerID = Guid.Parse("01850e6f-0000-0000-0000-504944564944");
			if
			(
				productID_GUID == Guid.Parse("2009057e-0000-0000-0000-504944564944") ||// Switch Pro Controller
				productID_GUID == smashControllerID// Wired Smash controller for Switch
			)
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = DeviceTriggerButtonMode.Physical;

				// primary buttons
				if (productID_GUID == smashControllerID)
				{
					configuration.button1 = buttons[2];
					configuration.button2 = buttons[1];
					configuration.button3 = buttons[0];
					configuration.button4 = buttons[3];
					configuration.button1.name = "A";
					configuration.button2.name = "B";
					configuration.button3.name = "X";
					configuration.button4.name = "Y";
				}
				else
				{
					configuration.button1 = buttons[0];
					configuration.button2 = buttons[1];
					configuration.button3 = buttons[2];
					configuration.button4 = buttons[3];
					configuration.button1.name = "B";
					configuration.button2.name = "A";
					configuration.button3.name = "X";
					configuration.button4.name = "Y";
				}

				// special button
				configuration.special1 = buttons[13];
				configuration.special1.name = "Capture";

				// dpad
				configuration.dpadLeft = new Button(true);
				configuration.dpadRight = new Button(true);
				configuration.dpadDown = new Button(true);
				configuration.dpadUp = new Button(true);
				configuration.dpadLeft.name = "Left";
				configuration.dpadRight.name = "Right";
				configuration.dpadDown.name = "Down";
				configuration.dpadUp.name = "Up";

				// options
				configuration.menu = buttons[9];
				configuration.back = buttons[8];
				configuration.home = buttons[12];
				configuration.menu.name = "+";
				configuration.back.name = "-";
				configuration.home.name = "Home";

				// bumbers
				configuration.bumperLeft = buttons[4];
				configuration.bumperRight = buttons[5];
				configuration.bumperLeft.name = "BL";
				configuration.bumperRight.name = "BR";

				// trigger buttons
				configuration.triggerButtonLeft = buttons[6];
				configuration.triggerButtonRight = buttons[7];
				configuration.triggerButtonLeft.name = "TBL";
				configuration.triggerButtonRight.name = "TBR";

				// joystick buttons
				configuration.joystickButtonLeft = buttons[10];
				configuration.joystickButtonRight = buttons[11];
				configuration.joystickButtonLeft.name = "JBL";
				configuration.joystickButtonRight.name = "JBR";

				// joysticks
				configuration.joystickLeft = new Axis2D(true);
				configuration.joystickRight = new Axis2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX_Src = axes1D[0];
				configuration.axis2DMaps[0].axisY_Src = axes1D[1];
				configuration.axis2DMaps[0].axisDst = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX_Src = axes1D[2];
				configuration.axis2DMaps[1].axisY_Src = axes1D[3];
				configuration.axis2DMaps[1].axisDst = configuration.joystickRight;
			}
			else if (productID_GUID == Guid.Parse("18460079-0000-0000-0000-504944564944"))// Generic GameCube USB adapter for PC
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = DeviceTriggerButtonMode.Physical;

				// primary buttons
				configuration.button1 = buttons[1];
				configuration.button2 = buttons[2];
				configuration.button3 = buttons[3];
				configuration.button4 = buttons[0];
				configuration.button6 = buttons[7];
				configuration.button1.name = "A";
				configuration.button2.name = "B";
				configuration.button3.name = "X";
				configuration.button4.name = "Y";
				configuration.button6.name = "Z";

				// special button
				configuration.special1 = buttons[13];
				configuration.special1.name = "Touch-Pad";

				// dpad
				configuration.dpadLeft = new Button(true);
				configuration.dpadRight = new Button(true);
				configuration.dpadDown = new Button(true);
				configuration.dpadUp = new Button(true);
				configuration.dpadLeft.name = "Left";
				configuration.dpadRight.name = "Right";
				configuration.dpadDown.name = "Down";
				configuration.dpadUp.name = "Up";

				// options
				configuration.menu = buttons[9];
				configuration.menu.name = "Pause";

				// bumbers
				configuration.bumperLeft = buttons[4];
				configuration.bumperRight = buttons[5];
				configuration.bumperLeft.name = "BL";
				configuration.bumperRight.name = "BR";

				// trigger buttons
				configuration.triggerButtonLeft = buttons[4];
				configuration.triggerButtonRight = buttons[5];
				configuration.triggerButtonLeft.name = "TBL";
				configuration.triggerButtonRight.name = "TBR";

				// triggers
				configuration.triggerLeft = new Axis1D(true, Axis1DUpdateMode.FullRange_ShiftedPositive);
				configuration.triggerRight = new Axis1D(true, Axis1DUpdateMode.FullRange_ShiftedPositive);
				configuration.triggerLeft.name = "TL";
				configuration.triggerRight.name = "TR";

				configuration.axis1DMaps = new DeviceAxis1DMap[2];
				configuration.axis1DMaps[0].axisSrc = axes1D[3];
				configuration.axis1DMaps[0].axisDst = configuration.triggerLeft;
				configuration.axis1DMaps[1].axisSrc = axes1D[4];
				configuration.axis1DMaps[1].axisDst = configuration.triggerRight;

				// joysticks
				configuration.joystickLeft = new Axis2D(true);
				configuration.joystickRight = new Axis2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX_Src = axes1D[0];
				configuration.axis2DMaps[0].axisY_Src = axes1D[1];
				configuration.axis2DMaps[0].axisDst = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX_Src = axes1D[2];
				configuration.axis2DMaps[1].axisY_Src = axes1D[5];
				configuration.axis2DMaps[1].axisDst = configuration.joystickRight;
			}*/

			return hardwareConfigurations;
		}

		private GamepadConfiguration GamepadConfiguration_Micorsoft_Xbox(bool isXbox360)
		{
			var config = new GamepadConfiguration();

			config.dpad_POV_Index = 0;
			config.dpadMode = DeviceDPadMode.POV;
			config.triggerButtonMode = DeviceTriggerButtonMode.Virtual;

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
			config.dpadLeft.name = "Left";
			config.dpadRight.name = "Right";
			config.dpadDown.name = "Down";
			config.dpadUp.name = "Up";

			// options
			config.menu.index = 7;
			config.back.index = 6;
			config.menu.name = isXbox360 ? "Start" : "Menu";
			config.back.name = "Back";
			if (!isXbox360)// only some controllers supply this
			{
				config.home.index = 10;
				config.home.name = "Xbox";
			}

			// bumbers
			config.bumperLeft.index = 4;
			config.bumperRight.index = 5;
			config.bumperLeft.name = "BL";
			config.bumperRight.name = "BR";

			// trigger buttons
			config.triggerButtonLeft.name = "TBL";
			config.triggerButtonRight.name = "TBR";

			// joystick buttons
			config.joystickButtonLeft.index = 8;
			config.joystickButtonRight.index = 9;
			config.joystickButtonLeft.name = "JBL";
			config.joystickButtonRight.name = "JBR";

			// triggers
			config.triggerLeft_Axis1D.updateMode = Axis1DUpdateMode.Positive;
			config.triggerRight_Axis1D.updateMode = Axis1DUpdateMode.Negitive;
			config.triggerLeft_Axis1D.name = "TL";
			config.triggerRight_Axis1D.name = "TR";

			config.axis1DMaps = new DeviceMapConfig_Axis1D[2];
			config.axis1DMaps[0].axis1D_Src = 2;
			config.axis1DMaps[0].axis1D_Dst = GamepadConfiguration.MapConfig_Axis1D.triggerLeft;
			config.axis1DMaps[1].axis1D_Src = 2;
			config.axis1DMaps[1].axis1D_Dst = GamepadConfiguration.MapConfig_Axis1D.triggerRight;

			// joysticks
			config.joystickLeft_Axis2D.name = "JL";
			config.joystickRight_Axis2D.name = "JR";

			config.axis2DMaps = new DeviceMapConfig_Axis2D[2];
			config.axis2DMaps[0].invertAxisY = true;
			config.axis2DMaps[0].axis1D_X_Src = 0;
			config.axis2DMaps[0].axis1D_Y_Src = 1;
			config.axis2DMaps[0].axis2D_Dst = GamepadConfiguration.MapConfig_Axis2D.joystickLeft;

			config.axis2DMaps[1].invertAxisY = true;
			config.axis2DMaps[1].axis1D_X_Src = 3;
			config.axis2DMaps[1].axis1D_Y_Src = 4;
			config.axis2DMaps[1].axis2D_Dst = GamepadConfiguration.MapConfig_Axis2D.joystickRight;

			return config;
		}
	}
}
