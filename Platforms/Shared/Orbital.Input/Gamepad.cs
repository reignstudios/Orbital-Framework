using Orbital.Numerics;
using System;

namespace Orbital.Input
{
	public struct GamepadConfiguration
	{
		public static class MapConfig_Axis1D
		{
			public const int triggerLeft = 0;
			public const int triggerRight = 1;
		}

		public static class MapConfig_Axis2D
		{
			public const int joystickLeft = 0;
			public const int joystickRight = 1;
		}

		public int dpad_POV_Index;
		public DeviceDPadMode dpadMode;
		public DeviceTriggerButtonMode triggerButtonMode;

		public DeviceConfig_Object button1, button2, button3, button4, button5, button6;
		public DeviceConfig_Object special1, special2;
		public DeviceConfig_Object dpadLeft, dpadRight, dpadDown, dpadUp;
		public DeviceConfig_Object menu, back, home;
		public DeviceConfig_Object bumperLeft, bumperRight;
		public DeviceConfig_Object triggerButtonLeft, triggerButtonRight;
		public DeviceConfig_Object joystickButtonLeft, joystickButtonRight;

		public DeviceConfig_Axis1D triggerLeft_Axis1D, triggerRight_Axis1D;
		public DeviceConfig_Object joystickLeft_Axis2D, joystickRight_Axis2D;

		public DeviceMapConfig_Axis1D[] axis1DMaps;
		public DeviceMapConfig_Axis2D[] axis2DMaps;
	}

	public struct GamepadHardwareConfiguration
	{
		public int vendorID, productID;
		public GamepadConfiguration config;
	}

	public sealed class Gamepad : IDisposable
	{
		public DeviceBase device { get; private set; }

		/// <summary>
		/// Is true if device connected
		/// </summary>
		public bool connected { get { return device.connected; } }

		#region Common Buttons
		/// <summary>
		/// Common button: A (XB), X (PS), B {Nintendo), etc
		/// </summary>
		public Button button1 { get; protected set; }

		/// <summary>
		/// Common button: B (XB), O (PS), A {Nintendo), etc
		/// </summary>
		public Button button2 { get; protected set; }

		/// <summary>
		/// Common button: X (XB), □ (PS), Y {Nintendo), etc
		/// </summary>
		public Button button3 { get; protected set; }

		/// <summary>
		/// Common button: Y (XB), △ (PS), X {Nintendo), etc
		/// </summary>
		public Button button4 { get; protected set; }

		/// <summary>
		/// Common button: C (Sega, MS), etc
		/// </summary>
		public Button button5 { get; protected set; }

		/// <summary>
		/// Common button: Z (Sega, MS), etc
		/// </summary>
		public Button button6 { get; protected set; }

		/// <summary>
		/// Special button: Touch-Pad (Sony), etc
		/// </summary>
		public Button special1 { get; protected set; }

		/// <summary>
		/// Special button: Mute (Sony), etc
		/// </summary>
		public Button special2 { get; protected set; }

		/// <summary>
		/// Common button: DPad Left
		/// </summary>
		public Button dpadLeft { get; protected set; }

		/// <summary>
		/// Common button: DPad Right
		/// </summary>
		public Button dpadRight { get; protected set; }

		/// <summary>
		/// Common button: DPad Down
		/// </summary>
		public Button dpadDown { get; protected set; }

		/// <summary>
		/// Common button: DPad Up
		/// </summary>
		public Button dpadUp { get; protected set; }

		/// <summary>
		/// Special button: System menu, OS home, etc
		/// </summary>
		public Button home { get; protected set; }

		/// <summary>
		/// Common button: Menu, Start, Options, etc
		/// </summary>
		public Button menu { get; protected set; }

		/// <summary>
		/// Common button: Back, Select, etc
		/// </summary>
		public Button back { get; protected set; }

		/// <summary>
		/// Common button: Bumper Left
		/// </summary>
		public Button bumperLeft { get; protected set; }

		/// <summary>
		/// Common button: Bumper Right
		/// </summary>
		public Button bumperRight { get; protected set; }

		/// <summary>
		/// Common button: Trigger Button Left
		/// </summary>
		public Button triggerButtonLeft { get; protected set; }

		/// <summary>
		/// Common button: Trigger Button Right
		/// </summary>
		public Button triggerButtonRight { get; protected set; }

		/// <summary>
		/// Common button: Joystick Left
		/// </summary>
		public Button joystickButtonLeft { get; protected set; }

		/// <summary>
		/// Common button: Joystick Right
		/// </summary>
		public Button joystickButtonRight { get; protected set; }
		#endregion

		#region Common 1D Axes
		/// <summary>
		/// Common axes: Trigger Left
		/// </summary>
		public Axis1D triggerLeft { get; protected set; }

		/// <summary>
		/// Common axes: Trigger Right
		/// </summary>
		public Axis1D triggerRight { get; protected set; }
		#endregion

		#region Common 2D Axes
		/// <summary>
		/// Common axes: Joystick Left
		/// </summary>
		public Axis2D joystickLeft { get; protected set; }

		/// <summary>
		/// Common axes: Joystick Right
		/// </summary>
		public Axis2D joystickRight { get; protected set; }
		#endregion

		private int dpad_POV_Index;
		private DeviceDPadMode dpadMode = DeviceDPadMode.POV;
		private DeviceTriggerButtonMode triggerButtonMode = DeviceTriggerButtonMode.Virtual;
		private DeviceMap_Axis1D[] axis1DMaps;
		private DeviceMap_Axis2D[] axis2DMaps;

		private ButtonNameMap[] buttonNameMaps;
		private Axis1DNameMap[] triggerNameMaps;
		private Axis2DNameMap[] joystickNameMaps;

		public Gamepad(DeviceBase device)
		{
			this.device = device;
			device.UpdateAbstractionCallback += Device_UpdateAbstractionCallback;
		}

		public void Dispose()
		{
			device.UpdateAbstractionCallback -= Device_UpdateAbstractionCallback;
		}

		private void Device_UpdateAbstractionCallback()
		{
			// dpad (clock-wise 0-36000)
			if (dpadMode == DeviceDPadMode.POV && device.povDirections.Length != 0)
			{
				if (dpad_POV_Index < 0) dpad_POV_Index = 0;
				if (dpad_POV_Index >= device.povDirections.Length) dpad_POV_Index = device.povDirections.Length - 1;
				uint value = device.povDirections[dpad_POV_Index];
				dpadUp.Update((value >= 31500 && value <= 36000) || (value >= 0 && value < 4500));
				dpadRight.Update(value >= 4500 && value < 13500);
				dpadDown.Update(value >= 13500 && value < 22500);
				dpadLeft.Update(value >= 22500 && value < 31500);
			}

			// axes mappings
			if (axis1DMaps != null)
			{
				foreach (var map in axis1DMaps)
				{
					float value = map.axisSrc.value;
					if (map.invertSrc) value = -value;
					map.axisDst.Update(value);
				}
			}

			if (axis2DMaps != null)
			{
				foreach (var map in axis2DMaps)
				{
					var value = new Vec2(map.axisX_Src.value, map.axisY_Src.value);
					if (map.invertAxisX) value.x = -value.x;
					if (map.invertAxisY) value.y = -value.y;
					map.axisDst.Update(value);
				}
			}

			// trigger buttons (NOTE: must be ran after axis mappings)
			if (triggerButtonMode == DeviceTriggerButtonMode.Virtual)
			{
				triggerButtonLeft.Update(triggerLeft.value >= .75f);
				triggerButtonRight.Update(triggerRight.value >= .75f);
			}
		}

		/// <summary>
		/// Configure device with specific settings
		/// </summary>
		/// <param name="configuration">Configuration to use</param>
		public void Configure(GamepadConfiguration configuration)
		{
			// copy mode settings
			dpad_POV_Index = configuration.dpad_POV_Index;
			dpadMode = configuration.dpadMode;
			triggerButtonMode = configuration.triggerButtonMode;

			// primary buttons
			button1 = device.GetOrCreate_Button(configuration.button1.index);
			button2 = device.GetOrCreate_Button(configuration.button2.index);
			button3 = device.GetOrCreate_Button(configuration.button3.index);
			button4 = device.GetOrCreate_Button(configuration.button4.index);
			button5 = device.GetOrCreate_Button(configuration.button5.index);
			button6 = device.GetOrCreate_Button(configuration.button6.index);

			// special
			special1 = device.GetOrCreate_Button(configuration.special1.index);
			special2 = device.GetOrCreate_Button(configuration.special2.index);

			// dpad
			dpadLeft = device.GetOrCreate_Button(configuration.dpadLeft.index);
			dpadRight = device.GetOrCreate_Button(configuration.dpadRight.index);
			dpadDown = device.GetOrCreate_Button(configuration.dpadDown.index);
			dpadUp = device.GetOrCreate_Button(configuration.dpadUp.index);

			// options
			menu = device.GetOrCreate_Button(configuration.menu.index);
			back = device.GetOrCreate_Button(configuration.back.index);
			home = device.GetOrCreate_Button(configuration.home.index);

			// bumbers
			bumperLeft = device.GetOrCreate_Button(configuration.bumperLeft.index);
			bumperRight = device.GetOrCreate_Button(configuration.bumperRight.index);

			// trigger buttons
			triggerButtonLeft = device.GetOrCreate_Button(configuration.triggerButtonLeft.index);
			triggerButtonRight = device.GetOrCreate_Button(configuration.triggerButtonRight.index);

			// joystick buttons
			joystickButtonLeft = device.GetOrCreate_Button(configuration.joystickButtonLeft.index);
			joystickButtonRight = device.GetOrCreate_Button(configuration.joystickButtonRight.index);

			// triggers
			triggerLeft = device.GetOrCreate_Axis1D(configuration.triggerLeft_Axis1D.index, configuration.triggerLeft_Axis1D.updateMode);
			triggerRight = device.GetOrCreate_Axis1D(configuration.triggerRight_Axis1D.index, configuration.triggerRight_Axis1D.updateMode);

			// joysticks
			joystickLeft = device.GetOrCreate_Axis2D(configuration.joystickLeft_Axis2D.index);
			joystickRight = device.GetOrCreate_Axis2D(configuration.joystickRight_Axis2D.index);

			// configure maps
			if (configuration.axis1DMaps != null)
			{
				axis1DMaps = new DeviceMap_Axis1D[configuration.axis1DMaps.Length];
				for (int i = 0; i != axis1DMaps.Length; ++i)
				{
					switch (configuration.axis1DMaps[i].axis1D_Dst)
					{
						case GamepadConfiguration.MapConfig_Axis1D.triggerLeft: axis1DMaps[i] = device.CreateAxis1DMap(configuration.axis1DMaps[i], triggerLeft); break;
						case GamepadConfiguration.MapConfig_Axis1D.triggerRight: axis1DMaps[i] = device.CreateAxis1DMap(configuration.axis1DMaps[i], triggerRight); break;
						default: throw new NotSupportedException("Axis1D map out of bounds: " + i.ToString());
					}
				}
			}

			if (configuration.axis2DMaps != null)
			{
				axis2DMaps = new DeviceMap_Axis2D[configuration.axis2DMaps.Length];
				for (int i = 0; i != axis2DMaps.Length; ++i)
				{
					switch (configuration.axis2DMaps[i].axis2D_Dst)
					{
						case GamepadConfiguration.MapConfig_Axis2D.joystickLeft: axis2DMaps[i] = device.CreateAxis2DMap(configuration.axis2DMaps[i], joystickLeft); break;
						case GamepadConfiguration.MapConfig_Axis2D.joystickRight: axis2DMaps[i] = device.CreateAxis2DMap(configuration.axis2DMaps[i], joystickRight); break;
						default: throw new NotSupportedException("Axis1D map out of bounds: " + i.ToString());
					}
				}
			}

			// map config names >>>
			int m = 0;

			// <<< buttons
			buttonNameMaps = new ButtonNameMap[21];
			buttonNameMaps[m++] = new ButtonNameMap(button1, configuration.button1.name);
			buttonNameMaps[m++] = new ButtonNameMap(button2, configuration.button2.name);
			buttonNameMaps[m++] = new ButtonNameMap(button3, configuration.button3.name);
			buttonNameMaps[m++] = new ButtonNameMap(button4, configuration.button4.name);
			buttonNameMaps[m++] = new ButtonNameMap(button5, configuration.button5.name);
			buttonNameMaps[m++] = new ButtonNameMap(button6, configuration.button6.name);

			buttonNameMaps[m++] = new ButtonNameMap(special1, configuration.special1.name);
			buttonNameMaps[m++] = new ButtonNameMap(special2, configuration.special2.name);

			buttonNameMaps[m++] = new ButtonNameMap(dpadLeft, configuration.dpadLeft.name);
			buttonNameMaps[m++] = new ButtonNameMap(dpadRight, configuration.dpadRight.name);
			buttonNameMaps[m++] = new ButtonNameMap(dpadDown, configuration.dpadDown.name);
			buttonNameMaps[m++] = new ButtonNameMap(dpadUp, configuration.dpadUp.name);

			buttonNameMaps[m++] = new ButtonNameMap(menu, configuration.menu.name);
			buttonNameMaps[m++] = new ButtonNameMap(back, configuration.back.name);
			buttonNameMaps[m++] = new ButtonNameMap(home, configuration.home.name);

			buttonNameMaps[m++] = new ButtonNameMap(bumperLeft, configuration.bumperLeft.name);
			buttonNameMaps[m++] = new ButtonNameMap(bumperRight, configuration.bumperRight.name);

			buttonNameMaps[m++] = new ButtonNameMap(triggerButtonLeft, configuration.triggerButtonLeft.name);
			buttonNameMaps[m++] = new ButtonNameMap(triggerButtonRight, configuration.triggerButtonRight.name);

			buttonNameMaps[m++] = new ButtonNameMap(joystickButtonLeft, configuration.joystickButtonLeft.name);
			buttonNameMaps[m++] = new ButtonNameMap(joystickButtonRight, configuration.joystickButtonRight.name);

			// <<< triggers
			triggerNameMaps = new Axis1DNameMap[2];
			triggerNameMaps[0] = new Axis1DNameMap(triggerLeft, configuration.triggerLeft_Axis1D.name);
			triggerNameMaps[1] = new Axis1DNameMap(triggerRight, configuration.triggerRight_Axis1D.name);

			// <<< joysticks
			joystickNameMaps = new Axis2DNameMap[2];
			joystickNameMaps[0] = new Axis2DNameMap(joystickLeft, configuration.joystickLeft_Axis2D.name);
			joystickNameMaps[1] = new Axis2DNameMap(joystickRight, configuration.joystickRight_Axis2D.name);
		}

		/// <summary>
		/// Tries to configure input based on productID & vendorID
		/// </summary>
		/// <param name="configurations">Configurations to test against</param>
		/// <returns>True if successful</returns>
		public bool Configure(GamepadHardwareConfiguration[] configurations)
		{
			int index = FindHardwareConfiguration(device, configurations);
			if (index < 0) return false;
			Configure(configurations[index].config);
			return true;
		}

		/// <summary>
		/// Tries to configure input based on productID & vendorID
		/// </summary>
		/// <param name="device">Device to check against</param>
		/// <param name="configurations">Configurations to test against device</param>
		/// <returns>Index of configuration</returns>
		public static int FindHardwareConfiguration(DeviceBase device, GamepadHardwareConfiguration[] configurations)
		{
			if (device == null || configurations == null) return -1;
			for (int i = 0; i != configurations.Length; ++i)
			{
				if (configurations[i].productID == device.productID && (configurations[i].vendorID == 0 || configurations[i].vendorID == device.vendorID))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Sets all motors rumble on supported controllers
		/// </summary>
		/// <param name="value">0-1 min-max value</param>
		public void SetRumble(float value)
		{
			device.SetRumble(value);
		}

		/// <summary>
		/// Sets motor rumble on supported controllers
		/// </summary>
		/// <param name="leftValue">0-1 min-max value on left motor</param>
		/// <param name="rightValue">0-1 min-max value on right motor</param>
		public void SetRumble(float leftValue, float rightValue)
		{
			device.SetRumble(leftValue, rightValue);
		}

		/// <summary>
		/// Sets motor rumble on supported controllers
		/// </summary>
		/// <param name="value">0-1 min-max value</param>
		/// <param name="motorIndex">Motor index</param>
		public void SetRumble(float value, int motorIndex)
		{
			device.SetRumble(value, motorIndex);
		}

		/// <summary>
		/// Searches for Button config name. (returns null if not found)
		/// </summary>
		/// <param name="button">Button part of this Gamepad</param>
		/// <returns>Button name</returns>
		public string GetButtonName(Button button)
		{
			foreach (var map in buttonNameMaps)
			{
				if (map.button == button) return map.name;
			}
			return null;
		}

		/// <summary>
		/// Searches for Trigger config name. (returns null if not found)
		/// </summary>
		/// <param name="trigger">Trigger part of this Gamepad</param>
		/// <returns>Trigger name</returns>
		public string GetTriggerName(Axis1D trigger)
		{
			foreach (var map in triggerNameMaps)
			{
				if (map.axis == trigger) return map.name;
			}
			return null;
		}

		/// <summary>
		/// Searches for Joystick config name. (returns null if not found)
		/// </summary>
		/// <param name="joystick">Joystick part of this Gamepad</param>
		/// <returns>Joystick name</returns>
		public string GetJoystickName(Axis2D joystick)
		{
			foreach (var map in joystickNameMaps)
			{
				if (map.axis == joystick) return map.name;
			}
			return null;
		}
	}


}
