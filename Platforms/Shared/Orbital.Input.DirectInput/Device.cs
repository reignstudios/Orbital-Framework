using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

namespace Orbital.Input.DirectInput
{
	public enum DeviceHardware
	{
		/// <summary>
		/// Unknown device
		/// </summary>
		Unknown,

		/// <summary>
		/// Microsoft Xbox 360 gamepad
		/// </summary>
		Xbox360_Gamepad,

		/// <summary>
		/// Microsoft Xbox One gamepad
		/// </summary>
		XboxOne_Gamepad,

		/// <summary>
		/// Microsoft Xbox SX/SS gamepad
		/// </summary>
		XboxSeries_Gamepad,

		/// <summary>
		/// Sony PS3 gamepad
		/// </summary>
		PS3_Gamepad,

		/// <summary>
		/// Sony PS4 gamepad
		/// </summary>
		PS4_Gamepad
	}

	public enum DeviceDPadMode
	{
		/// <summary>
		/// Dpad maps buttons from POV (point-of-view hats)
		/// </summary>
		POV,

		/// <summary>
		/// Dpad is a set of normal buttons
		/// </summary>
		Buttons
	}

	public enum DeviceTriggerButtonMode
	{
		/// <summary>
		/// Buttons are activated when trigger goes over 75%
		/// </summary>
		Virtual,

		/// <summary>
		/// Trigger buttons exist physically on the hardware
		/// </summary>
		Physical
	}

	public struct DeviceAxis1DMap
	{
		public bool invertSrc;
		public Analog1D analogSrc;
		public Analog1D analogDst;
	}

	public struct DeviceAxis2DMap
	{
		public bool invertAxisX, invertAxisY;
		public Analog1D axisX, axisY;
		public Analog2D analog;
	}

	public struct DeviceAxis3DMap
	{
		public bool invertAxisX, invertAxisY, invertAxisZ;
		public Analog1D axisX, axisY, axisZ;
		public Analog3D analog;
	}

	public struct InputConfiguration
	{
		public int dpad_POV_Index;
		public DeviceDPadMode dpadMode;
		public DeviceTriggerButtonMode triggerButtonMode;

		public Button button1, button2, button3, button4, button5, button6;
		public Button special1, special2;
		public Button dpadLeft, dpadRight, dpadDown, dpadUp;
		public Button menu, back, home;
		public Button bumperLeft, bumperRight;
		public Button triggerButtonLeft, triggerButtonRight;
		public Button joystickButtonLeft, joystickButtonRight;

		public Analog1D triggerLeft, triggerRight;
		public Analog2D joystickLeft, joystickRight;

		public DeviceAxis1DMap[] axis1DMaps;
		public DeviceAxis2DMap[] axis2DMaps;
		public DeviceAxis3DMap[] axis3DMaps;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct DeviceInfo
	{
		public Guid productID;
		public unsafe char* productName;
		public int supportsForceFeedback;
		public int isPrimary;

		public int buttonCount;
		public int keyCount;
		public int povCount;
		public int sliderCount;
		public int xAxisCount, yAxisCount, zAxisCount;
		public int rxAxisCount, ryAxisCount, rzAxisCount;
	}

	public sealed class Device : DeviceBase
	{
		public Instance instanceDI { get; private set; }

		/// <summary>
		/// Primary device configured in Windows control panel
		/// </summary>
		public bool isPrimary { get; private set; }

		/// <summary>
		/// Product ID from manufacture
		/// </summary>
		public Guid productID { get; private set; }

		/// <summary>
		/// Product name from manufacture
		/// </summary>
		public string productName { get; private set; }

		/// <summary>
		/// Does this device support force-feedback / haptics
		/// </summary>
		public bool supportsForceFeedback { get; private set; }

		/// <summary>
		/// The device index
		/// </summary>
		public int index { get; private set; }

		private DeviceInfo nativeInfo;

		private int dpad_POV_Index;
		private DeviceDPadMode dpadMode = DeviceDPadMode.POV;
		private DeviceTriggerButtonMode triggerButtonMode = DeviceTriggerButtonMode.Virtual;

		private DeviceAxis1DMap[] axis1DMaps;
		private DeviceAxis2DMap[] axis2DMaps;
		private DeviceAxis3DMap[] axis3DMaps;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern int Orbital_Video_DirectInput_Instance_GetDeviceState(IntPtr handle, int deviceIndex, DIJOYSTATE2* state, int* connected);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern void Orbital_Video_DirectInput_Instance_GetDeviceInfo(IntPtr handle, int deviceIndex, DeviceInfo* info);

		public Device(Instance instance, int index)
		: base(instance)
		{
			instanceDI = instance;
			this.index = index;
		}

		public unsafe void Init()
		{
			// get device info
			DeviceInfo info;
			Orbital_Video_DirectInput_Instance_GetDeviceInfo(instanceDI.handle, index, &info);
			nativeInfo = info;
			productID = info.productID;
			productName = new string(info.productName);
			supportsForceFeedback = info.supportsForceFeedback != 0;
			isPrimary = info.isPrimary != 0;

			// get total axis count
			int axisCount = info.xAxisCount + info.yAxisCount + info.zAxisCount;
			axisCount += info.rxAxisCount + info.ryAxisCount + info.rzAxisCount;

			// create objects
			CreatePhysicalObjects(info.buttonCount, axisCount, 0, 0, info.sliderCount);

			// configure input settings
			var configuration = new InputConfiguration();

			// ================
			// Microsoft
			// ================
			var xbox360ID = Guid.Parse("028e045e-0000-0000-0000-504944564944");
			var logitechXInputID = Guid.Parse("c21d046d-0000-0000-0000-504944564944");// Logitech XInput mode
			var logitechDirecInputID = Guid.Parse("c216046d-0000-0000-0000-504944564944");// Logitech DirectInput mode
			bool isXbox360 = productID == xbox360ID || productID == logitechXInputID;
			if
			(
				isXbox360 ||// Xbox 360
				productID == Guid.Parse("02ff045e-0000-0000-0000-504944564944")// Xbox One
			)
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = DeviceTriggerButtonMode.Virtual;

				// primary buttons
				configuration.button1 = buttons[0];
				configuration.button2 = buttons[1];
				configuration.button3 = buttons[2];
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
				configuration.menu = buttons[7];
				configuration.back = buttons[6];
				configuration.menu.name = isXbox360 ? "Start" : "Menu";
				configuration.back.name = "Back";
				if (!isXbox360 && buttons.Count >= 11)// only some controllers supply this
				{
					configuration.home = buttons[10];
					configuration.home.name = "Xbox";
				}

				// bumbers
				configuration.bumperLeft = buttons[4];
				configuration.bumperRight = buttons[5];
				configuration.bumperLeft.name = "BL";
				configuration.bumperRight.name = "BR";

				// trigger buttons
				configuration.triggerButtonLeft = new Button(false);
				configuration.triggerButtonRight = new Button(false);
				configuration.triggerButtonLeft.name = "TBL";
				configuration.triggerButtonRight.name = "TBR";

				// joystick buttons
				configuration.joystickButtonLeft = buttons[8];
				configuration.joystickButtonRight = buttons[9];
				configuration.joystickButtonLeft.name = "JBL";
				configuration.joystickButtonRight.name = "JBR";

				// triggers
				configuration.triggerLeft = new Analog1D(true, Analog1DUpdateMode.Positive);
				configuration.triggerRight = new Analog1D(true, Analog1DUpdateMode.Negitive);
				configuration.triggerLeft.name = "TL";
				configuration.triggerRight.name = "TR";

				configuration.axis1DMaps = new DeviceAxis1DMap[2];
				configuration.axis1DMaps[0].analogSrc = analogs_1D[2];
				configuration.axis1DMaps[0].analogDst = configuration.triggerLeft;
				configuration.axis1DMaps[1].analogSrc = analogs_1D[2];
				configuration.axis1DMaps[1].analogDst = configuration.triggerRight;

				// joysticks
				configuration.joystickLeft = new Analog2D(true);
				configuration.joystickRight = new Analog2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX = analogs_1D[0];
				configuration.axis2DMaps[0].axisY = analogs_1D[1];
				configuration.axis2DMaps[0].analog = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX = analogs_1D[3];
				configuration.axis2DMaps[1].axisY = analogs_1D[4];
				configuration.axis2DMaps[1].analog = configuration.joystickRight;
			}
			else if (productID == logitechDirecInputID)// Logitech DirectInput mode
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
				configuration.joystickLeft = new Analog2D(true);
				configuration.joystickRight = new Analog2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX = analogs_1D[0];
				configuration.axis2DMaps[0].axisY = analogs_1D[1];
				configuration.axis2DMaps[0].analog = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX = analogs_1D[2];
				configuration.axis2DMaps[1].axisY = analogs_1D[3];
				configuration.axis2DMaps[1].analog = configuration.joystickRight;
			}

			// ================
			// Sony
			// ================
			var ps3ID_Wireless = Guid.Parse("0268054c-0000-0000-0000-504944564944");
			var ps3ID_Wired = Guid.Parse("63020e6f-0000-0000-0000-504944564944");
			var ps5ID = Guid.Parse("0ce6054c-0000-0000-0000-504944564944");
			if
			(
				productID == Guid.Parse("05c4054c-0000-0000-0000-504944564944") ||// PS4
				productID == ps5ID ||// PS5
				productID == ps3ID_Wired// Wired PS3 controller
			)
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = productID == ps3ID_Wired ? DeviceTriggerButtonMode.Physical : DeviceTriggerButtonMode.Virtual;

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
				if (productID != ps3ID_Wired)
				{
					configuration.special1 = buttons[13];
					configuration.special1.name = "Touch-Pad";
				}

				if (productID == ps5ID)
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
				if (productID == ps3ID_Wired)
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
				if (productID == ps3ID_Wired)
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
				if (productID != ps3ID_Wired)
				{
					configuration.triggerLeft = new Analog1D(true, Analog1DUpdateMode.FullRange_ShiftedPositive);
					configuration.triggerRight = new Analog1D(true, Analog1DUpdateMode.FullRange_ShiftedPositive);
					configuration.triggerLeft.name = "TL";
					configuration.triggerRight.name = "TR";

					configuration.axis1DMaps = new DeviceAxis1DMap[2];
					configuration.axis1DMaps[0].analogSrc = analogs_1D[3];
					configuration.axis1DMaps[0].analogDst = configuration.triggerLeft;
					configuration.axis1DMaps[1].analogSrc = analogs_1D[4];
					configuration.axis1DMaps[1].analogDst = configuration.triggerRight;
				}

				// joysticks
				configuration.joystickLeft = new Analog2D(true);
				configuration.joystickRight = new Analog2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX = analogs_1D[0];
				configuration.axis2DMaps[0].axisY = analogs_1D[1];
				configuration.axis2DMaps[0].analog = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX = analogs_1D[2];
				if (productID == ps3ID_Wired) configuration.axis2DMaps[1].axisY = analogs_1D[3];
				else configuration.axis2DMaps[1].axisY = analogs_1D[5];
				configuration.axis2DMaps[1].analog = configuration.joystickRight;
			}
			else if (productID == ps3ID_Wireless)
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
				configuration.joystickLeft = new Analog2D(true);
				configuration.joystickRight = new Analog2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX = analogs_1D[0];
				configuration.axis2DMaps[0].axisY = analogs_1D[1];
				configuration.axis2DMaps[0].analog = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX = analogs_1D[2];
				configuration.axis2DMaps[1].axisY = analogs_1D[3];
				configuration.axis2DMaps[1].analog = configuration.joystickRight;
			}

			// ================
			// Nintendo
			// ================
			var smashControllerID = Guid.Parse("01850e6f-0000-0000-0000-504944564944");
			if
			(
				productID == Guid.Parse("2009057e-0000-0000-0000-504944564944") ||// Switch Pro Controller
				productID == smashControllerID// Wired Smash controller for Switch
			)
			{
				configuration.dpad_POV_Index = 0;
				configuration.dpadMode = DeviceDPadMode.POV;
				configuration.triggerButtonMode = DeviceTriggerButtonMode.Physical;

				// primary buttons
				if (productID == smashControllerID)
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
				configuration.joystickLeft = new Analog2D(true);
				configuration.joystickRight = new Analog2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX = analogs_1D[0];
				configuration.axis2DMaps[0].axisY = analogs_1D[1];
				configuration.axis2DMaps[0].analog = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX = analogs_1D[2];
				configuration.axis2DMaps[1].axisY = analogs_1D[3];
				configuration.axis2DMaps[1].analog = configuration.joystickRight;
			}
			else if (productID == Guid.Parse("18460079-0000-0000-0000-504944564944"))// GameCube
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
				configuration.triggerLeft = new Analog1D(true, Analog1DUpdateMode.FullRange_ShiftedPositive);
				configuration.triggerRight = new Analog1D(true, Analog1DUpdateMode.FullRange_ShiftedPositive);
				configuration.triggerLeft.name = "TL";
				configuration.triggerRight.name = "TR";

				configuration.axis1DMaps = new DeviceAxis1DMap[2];
				configuration.axis1DMaps[0].analogSrc = analogs_1D[3];
				configuration.axis1DMaps[0].analogDst = configuration.triggerLeft;
				configuration.axis1DMaps[1].analogSrc = analogs_1D[4];
				configuration.axis1DMaps[1].analogDst = configuration.triggerRight;

				// joysticks
				configuration.joystickLeft = new Analog2D(true);
				configuration.joystickRight = new Analog2D(true);
				configuration.joystickLeft.name = "JL";
				configuration.joystickRight.name = "JR";

				configuration.axis2DMaps = new DeviceAxis2DMap[2];
				configuration.axis2DMaps[0].invertAxisY = true;
				configuration.axis2DMaps[0].axisX = analogs_1D[0];
				configuration.axis2DMaps[0].axisY = analogs_1D[1];
				configuration.axis2DMaps[0].analog = configuration.joystickLeft;

				configuration.axis2DMaps[1].invertAxisY = true;
				configuration.axis2DMaps[1].axisX = analogs_1D[2];
				configuration.axis2DMaps[1].axisY = analogs_1D[5];
				configuration.axis2DMaps[1].analog = configuration.joystickRight;
			}

			// configure input
			Configure(ref configuration);

			// create any missing objects this API doesn't support
			CreateMissingObjects();
		}

		/// <summary>
		/// Custom device configuration mapping
		/// </summary>
		/// <param name="configuration">Configuration object to use</param>
		public void Configure(ref InputConfiguration configuration)
		{
			// get total axis count
			int axisCount = nativeInfo.xAxisCount + nativeInfo.yAxisCount + nativeInfo.zAxisCount;
			axisCount += nativeInfo.rxAxisCount + nativeInfo.ryAxisCount + nativeInfo.rzAxisCount;

			// remove non-native backed objects
			int delta = nativeInfo.buttonCount - buttons.Count;
			if (delta > 0) buttons_backing.RemoveRange(nativeInfo.buttonCount, delta);

			delta = axisCount - analogs_1D.Count;
			if (delta > 0) analogs_1D_backing.RemoveRange(axisCount, delta);

			// copy mode settings
			dpad_POV_Index = configuration.dpad_POV_Index;
			dpadMode = configuration.dpadMode;
			triggerButtonMode = configuration.triggerButtonMode;

			// copy maps
			if (configuration.axis1DMaps != null)
			{
				axis1DMaps = new DeviceAxis1DMap[configuration.axis1DMaps.Length];
				Array.Copy(configuration.axis1DMaps, axis1DMaps, axis1DMaps.Length);
			}

			if (configuration.axis2DMaps != null)
			{
				axis2DMaps = new DeviceAxis2DMap[configuration.axis2DMaps.Length];
				Array.Copy(configuration.axis2DMaps, axis2DMaps, axis2DMaps.Length);
			}

			if (configuration.axis3DMaps != null)
			{
				axis3DMaps = new DeviceAxis3DMap[configuration.axis3DMaps.Length];
				Array.Copy(configuration.axis3DMaps, axis3DMaps, axis3DMaps.Length);
			}

			// primary buttons
			button1 = configuration.button1;
			button2 = configuration.button2;
			button3 = configuration.button3;
			button4 = configuration.button4;
			button5 = configuration.button5;
			button6 = configuration.button6;

			// special
			special1 = configuration.special1;
			special2 = configuration.special2;

			// dpad
			dpadLeft = configuration.dpadLeft;
			dpadRight = configuration.dpadRight;
			dpadDown = configuration.dpadDown;
			dpadUp = configuration.dpadUp;

			// options
			menu = configuration.menu;
			back = configuration.back;
			home = configuration.home;

			// bumbers
			bumperLeft = configuration.bumperLeft;
			bumperRight = configuration.bumperRight;

			// trigger buttons
			triggerButtonLeft = configuration.triggerButtonLeft;
			triggerButtonRight = configuration.triggerButtonRight;

			// joystick buttons
			joystickButtonLeft = configuration.joystickButtonLeft;
			joystickButtonRight = configuration.joystickButtonRight;

			// triggers
			triggerLeft = configuration.triggerLeft;
			triggerRight = configuration.triggerRight;

			// joysticks
			joystickLeft = configuration.joystickLeft;
			joystickRight = configuration.joystickRight;

			// create any missing objects this API doesn't support (NOTE: call this before we try to add virtual objects)
			CreateMissingObjects();

			// primary buttons
			AddVirtualObject(button1);
			AddVirtualObject(button2);
			AddVirtualObject(button3);
			AddVirtualObject(button4);
			AddVirtualObject(button5);
			AddVirtualObject(button6);

			// dpad
			AddVirtualObject(dpadLeft);
			AddVirtualObject(dpadRight);
			AddVirtualObject(dpadDown);
			AddVirtualObject(dpadUp);

			// options
			AddVirtualObject(menu);
			AddVirtualObject(back);
			AddVirtualObject(home);

			// bumbers
			AddVirtualObject(bumperLeft);
			AddVirtualObject(bumperRight);

			// trigger buttons
			AddVirtualObject(triggerButtonLeft);
			AddVirtualObject(triggerButtonRight);

			// ensure analog map objects are added
			if (configuration.axis1DMaps != null)
			{
				foreach (var map in configuration.axis1DMaps)
				{
					AddVirtualObject(map.analogSrc);
					AddVirtualObject(map.analogDst);
				}
			}

			if (configuration.axis2DMaps != null)
			{
				foreach (var map in configuration.axis2DMaps)
				{
					AddVirtualObject(map.axisX);
					AddVirtualObject(map.axisY);
					AddVirtualObject(map.analog);
				}
			}

			if (configuration.axis3DMaps != null)
			{
				foreach (var map in configuration.axis3DMaps)
				{
					AddVirtualObject(map.axisX);
					AddVirtualObject(map.axisY);
					AddVirtualObject(map.axisZ);
					AddVirtualObject(map.analog);
				}
			}
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

			// update all buttons
			for (int i = 0; i != nativeInfo.buttonCount; ++i)
			{
				buttons[i].Update(state.rgbButtons[i] != 0);
			}

			// update all analogs1D
			int axisCount = nativeInfo.xAxisCount + nativeInfo.yAxisCount + nativeInfo.zAxisCount;
			axisCount += nativeInfo.rxAxisCount + nativeInfo.ryAxisCount + nativeInfo.rzAxisCount;

			int ax = 0;
			if (nativeInfo.xAxisCount != 0) analogs_1D[ax++].Update(state.lX / 1000f);
			if (nativeInfo.yAxisCount != 0) analogs_1D[ax++].Update(state.lY / 1000f);
			if (nativeInfo.zAxisCount != 0) analogs_1D[ax++].Update(state.lZ / 1000f);
			if (nativeInfo.rxAxisCount != 0) analogs_1D[ax++].Update(state.lRx / 1000f);
			if (nativeInfo.ryAxisCount != 0) analogs_1D[ax++].Update(state.lRy / 1000f);
			if (nativeInfo.rzAxisCount != 0) analogs_1D[ax++].Update(state.lRz / 1000f);

			// dpad (clock-wise 0-36000)
			if (dpadMode == DeviceDPadMode.POV && nativeInfo.povCount != 0)
			{
				if (dpad_POV_Index < 0) dpad_POV_Index = 0;
				if (dpad_POV_Index >= nativeInfo.povCount) dpad_POV_Index = nativeInfo.povCount - 1;
				uint value = state.rgdwPOV[dpad_POV_Index];
				dpadUp.Update((value >= 31500 && value <= 36000) || (value >= 0 && value < 4500));
				dpadRight.Update(value >= 4500 && value < 13500);
				dpadDown.Update(value >= 13500 && value < 22500);
				dpadLeft.Update(value >= 22500 && value < 31500);
			}

			// analog mappings
			if (axis1DMaps != null)
			{
				foreach (var map in axis1DMaps)
				{
					float value = map.analogSrc.value;
					if (map.invertSrc) value = -value;
					map.analogDst.Update(value);
				}
			}

			if (axis2DMaps != null)
			{
				foreach (var map in axis2DMaps)
				{
					var value = new Vec2(map.axisX.value, map.axisY.value);
					if (map.invertAxisX) value.x = -value.x;
					if (map.invertAxisY) value.y = -value.y;
					map.analog.Update(value);
				}
			}

			// analog3D mappings
			if (axis3DMaps != null)
			{
				foreach (var map in axis3DMaps)
				{
					var value = new Vec3(map.axisX.value, map.axisY.value, map.axisZ.value);
					if (map.invertAxisX) value.x = -value.x;
					if (map.invertAxisY) value.y = -value.y;
					if (map.invertAxisZ) value.z = -value.z;
					map.analog.Update(value);
				}
			}

			// trigger buttons (NOTE: must be ran after analog mappings)
			if (triggerButtonMode == DeviceTriggerButtonMode.Virtual)
			{
				triggerButtonLeft.Update(triggerLeft.value >= .75f);
				triggerButtonRight.Update(triggerRight.value >= .75f);
			}
		}
	}
}
