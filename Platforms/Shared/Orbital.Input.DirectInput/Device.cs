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

	public enum DeviceTriggerMode
	{
		/// <summary>
		/// Each trigger as its own axis
		/// </summary>
		Seperate,

		/// <summary>
		/// Left & Right trigger effect same axis
		/// </summary>
		Shared
	}

	public enum DeviceTriggerSharedAxis
	{
		X_Position,
		Y_Position,
		Z_Position,

		X_Rotation,
		Y_Rotation,
		Z_Rotation,

		X_Velocity,
		Y_Velocity,
		Z_Velocity,

		X_AngularVelocity,
		Y_AngularVelocity,
		Z_AngularVelocity,

		X_Acceleration,
		Y_Acceleration,
		Z_Acceleration,

		X_AngularAcceleration,
		Y_AngularAcceleration,
		Z_AngularAcceleration,

		X_Force,
		Y_Force,
		Z_Force,

		X_Torque,
		Y_Torque,
		Z_Torque,
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

	class DpadMap
	{
		public int index;
		public Button left, right, down, up;
	}

	class InputConfiguration
	{
		public DpadMap dpadMap;
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

		/// <summary>
		/// How the dpad buttons are mapped
		/// </summary>
		public DeviceDPadMode dpadMode = DeviceDPadMode.POV;

		/// <summary>
		/// How the triggers map
		/// </summary>
		public DeviceTriggerMode triggerMode = DeviceTriggerMode.Seperate;

		/// <summary>
		/// If the trigger mode is 'Shared' what axis do they use
		/// </summary>
		public DeviceTriggerSharedAxis triggerSharedAxis = DeviceTriggerSharedAxis.Z_Position;

		/// <summary>
		/// How the trigger buttons are mapped
		/// </summary>
		public DeviceTriggerButtonMode triggerButtonMode = DeviceTriggerButtonMode.Virtual;

		/// <summary>
		/// Which
		/// </summary>
		public int dpad_POV_Offset;

		private DeviceInfo nativeInfo;
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

			// configure input settings
			if (productID == new Guid("02ff045e-0000-0000-0000-504944564944"))// Xbox
			{
				dpadMode = DeviceDPadMode.POV;
				triggerMode = DeviceTriggerMode.Shared;
				triggerSharedAxis = DeviceTriggerSharedAxis.Z_Position;
				triggerButtonMode = DeviceTriggerButtonMode.Virtual;
				int buttonCount = info.buttonCount + 4 + 2;// add dpad-POV & triggers
				axisCount += 2;// add triggers for mappings
				CreateAttachedArrays(buttonCount, axisCount, 2, 0, info.sliderCount);

				// primary buttons
				button1 = buttons[0];
				button2 = buttons[1];
				button3 = buttons[2];
				button4 = buttons[3];
				button1.name = "A";
				button2.name = "B";
				button3.name = "X";
				button4.name = "Y";

				// dpad
				int buttonIndex = buttonCount - (4 + 2);
				dpadLeft = buttons[buttonIndex++];
				dpadRight = buttons[buttonIndex++];
				dpadDown = buttons[buttonIndex++];
				dpadUp = buttons[buttonIndex++];
				dpadLeft.name = "Left";
				dpadRight.name = "Right";
				dpadDown.name = "Down";
				dpadUp.name = "Up";

				// options
				menu = buttons[7];
				back = buttons[6];
				home = buttons[10];
				menu.name = "Menu";
				back.name = "Back";
				home.name = "Dashboard";

				// bumbers
				bumperLeft = buttons[4];
				bumperRight = buttons[5];
				bumperLeft.name = "BL";
				bumperRight.name = "BR";

				// trigger buttons
				triggerButtonLeft = buttons[buttonIndex++];
				triggerButtonRight = buttons[buttonIndex++];
				triggerButtonLeft.name = "TBL";
				triggerButtonRight.name = "TBR";

				// joystick buttons
				joystickButtonLeft = buttons[8];
				joystickButtonRight = buttons[9];
				joystickButtonLeft.name = "JBL";
				joystickButtonRight.name = "JBR";

				// triggers
				triggerLeft = new Analog1D(true, Analog1DUpdateMode.Positive);
				triggerRight = new Analog1D(true, Analog1DUpdateMode.Negitive);
				triggerLeft.name = "TL";
				triggerRight.name = "TR";
				analogs_1D[axisCount - 2] = triggerLeft;
				analogs_1D[axisCount - 1] = triggerRight;

				axis1DMaps = new DeviceAxis1DMap[2];
				axis1DMaps[0].analogSrc = analogs_1D[2];
				axis1DMaps[0].analogDst = triggerLeft;
				axis1DMaps[1].analogSrc = analogs_1D[2];
				axis1DMaps[1].analogDst = triggerRight;

				// joysticks
				joystickLeft = analogs_2D[0];
				joystickRight = analogs_2D[1];
				joystickLeft.name = "JL";
				joystickRight.name = "JR";

				axis2DMaps = new DeviceAxis2DMap[2];
				axis2DMaps[0].invertAxisY = true;
				axis2DMaps[0].axisX = analogs_1D[0];
				axis2DMaps[0].axisY = analogs_1D[1];
				axis2DMaps[0].analog = joystickLeft;

				axis2DMaps[1].invertAxisY = true;
				axis2DMaps[1].axisX = analogs_1D[3];
				axis2DMaps[1].axisY = analogs_1D[4];
				axis2DMaps[1].analog = joystickRight;
			}
			else if (productID == Guid.Parse("05c4054c-0000-0000-0000-504944564944"))// PS4
			{
				/*dpadMode = DeviceDPadMode.POV;
				triggerMode = DeviceTriggerMode.Seperate;
				triggerSharedAxis = DeviceTriggerSharedAxis.Z_Position;
				int buttonCount = info.buttonCount + info.povCount + 2;// add dpad-POV & triggers
				CreateAttachedArrays(buttonCount, axisCount, 2, 0, info.sliderCount);

				// primary buttons
				button1 = buttons[0];
				button2 = buttons[1];
				button3 = buttons[2];
				button4 = buttons[3];
				button1.name = "X";
				button2.name = "O";
				button3.name = "□";
				button4.name = "△";

				// dpad
				dpadLeft = new Button(true);
				dpadRight = new Button(true);
				dpadDown = new Button(true);
				dpadUp = new Button(true);
				dpadLeft.name = "Left";
				dpadRight.name = "Right";
				dpadDown.name = "Down";
				dpadUp.name = "Up";

				// options
				menu = buttons[7];
				back = buttons[6];
				home = buttons[11];
				menu.name = "Options";
				back.name = "Share";
				home.name = "PS";

				// bumbers
				bumperLeft = buttons[4];
				bumperRight = buttons[4];
				bumperLeft.name = "BL";
				bumperRight.name = "BR";

				// trigger buttons
				triggerButtonLeft = buttons[4];
				triggerButtonRight = buttons[4];
				triggerButtonLeft.name = "TBL";
				triggerButtonRight.name = "TBR";

				// joystick buttons
				joystickButtonLeft = buttons[4];
				joystickButtonRight = buttons[4];
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
				joystickRight.name = "JR";*/
			}
			else
			{
				CreateAttachedArrays(info.buttonCount, axisCount, 0, 0, info.sliderCount);
			}

			// create any missing objects this API doesn't support
			CreateMissingObjects();
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
			//		Console.WriteLine($"{i} = {state.rgbButtons[i]}");
			//	}
			//}

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
				if (dpad_POV_Offset < 0) dpad_POV_Offset = 0;
				if (dpad_POV_Offset >= nativeInfo.povCount) dpad_POV_Offset = nativeInfo.povCount - 1;
				uint value = state.rgdwPOV[dpad_POV_Offset];
				dpadUp.Update((value >= 31500 && value <= 36000) || (value >= 0 && value < 4500));
				dpadRight.Update(value >= 4500 && value < 13500);
				dpadDown.Update(value >= 13500 && value < 22500);
				dpadLeft.Update(value >= 22500 && value < 31500);
			}

			// trigger buttons
			if (triggerButtonMode == DeviceTriggerButtonMode.Virtual)
			{
				float triggerLeftValue = state.lZ / 1000f;
				float triggerRightValue = -(state.lZ / 1000f);
				triggerButtonLeft.Update(triggerLeftValue >= .75f);
				triggerButtonRight.Update(triggerRightValue >= .75f);
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
		}
	}
}
