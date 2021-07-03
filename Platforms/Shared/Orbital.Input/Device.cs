using Orbital.Numerics;
using Orbital.Primitives;
using System;
using System.Collections.Generic;

namespace Orbital.Input
{
	public enum DeviceType
	{
		/// <summary>
		/// Unknown device
		/// </summary>
		Unknown,

		/// <summary>
		/// Common gamepad
		/// </summary>
		Gamepad,

		/// <summary>
		/// Fighting / Arcade style controllers
		/// </summary>
		ArcadeStick,

		/// <summary>
		/// Flight-Stick for plane or space-shit simulators
		/// </summary>
		FlightStick,

		/// <summary>
		/// Steering-Wheel for racing games etc
		/// </summary>
		SteeringWheel,

		/// <summary>
		/// Keyboard
		/// </summary>
		Keyboard,

		/// <summary>
		/// Mouse
		/// </summary>
		Mouse,

		/// <summary>
		/// Remote control for video players or UI navigation
		/// </summary>
		Remote,

		/// <summary>
		/// Light-gun, screen-pointer, etc
		/// </summary>
		Pointer
	}

	public enum DeviceHardware
	{
		/// <summary>
		/// Unknown device
		/// </summary>
		Unknown,

		/// <summary>
		/// Generic gamepad
		/// </summary>
		Generic_Gamepad,

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
		PS4_Gamepad,

		/// <summary>
		/// Sony PS5 gamepad
		/// </summary>
		PS5_Gamepad,
		
		/// <summary>
		/// Nintendo GameCube gamepad
		/// </summary>
		GameCube_Gamepas
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

	public struct DeviceConfig_Object
	{
		/// <summary>
		/// Index of button in Device
		/// </summary>
		public int? index;

		/// <summary>
		/// Name of object
		/// </summary>
		public string name;

		public DeviceConfig_Object(int? index, string name)
		{
			this.index = index;
			this.name = name;
		}
	}

	public struct DeviceConfig_Axis1D
	{
		/// <summary>
		/// Index of button in Device
		/// </summary>
		public int? index;

		/// <summary>
		/// Name of object
		/// </summary>
		public string name;

		/// <summary>
		/// How the update values are processed
		/// </summary>
		public Axis1DUpdateMode updateMode;

		public DeviceConfig_Axis1D(int? index, string name, Axis1DUpdateMode updateMode)
		{
			this.index = index;
			this.name = name;
			this.updateMode = updateMode;
		}
	}

	public struct DeviceMapConfig_Axis1D
	{
		public bool invertSrc;
		public int axis1D_Src;
		public int axis1D_Dst;
	}

	public struct DeviceMapConfig_Axis2D
	{
		public bool invertAxisX, invertAxisY;
		public int axis1D_X_Src, axis1D_Y_Src;
		public int axis2D_Dst;
	}

	public struct DeviceMapConfig_Axis3D
	{
		public bool invertAxisX, invertAxisY, invertAxisZ;
		public int axis1D_X_Src, axis1D_Y_Src, axis1D_Z_Src;
		public int axis3D_Dst;
	}

	public struct DeviceMap_Axis1D
	{
		public bool invertSrc;
		public Axis1D axisSrc;
		public Axis1D axisDst;
	}

	public struct DeviceMap_Axis2D
	{
		public bool invertAxisX, invertAxisY;
		public Axis1D axisX_Src, axisY_Src;
		public Axis2D axisDst;
	}

	public struct DeviceMap_Axis3D
	{
		public bool invertAxisX, invertAxisY, invertAxisZ;
		public Axis1D axisX_Src, axisY_Src, axisZ_Src;
		public Axis3D axisDst;
	}

	public abstract class DeviceBase
	{
		public InstanceBase instance { get; private set; }

		/// <summary>
		/// Name of device
		/// </summary>
		public string name { get; protected set; }

		/// <summary>
		/// Vendor ID from manufacture
		/// </summary>
		public ushort vendorID { get; protected set; }

		/// <summary>
		/// Product ID from manufacture
		/// </summary>
		public ushort productID { get; protected set; }

		/// <summary>
		/// Device type
		/// </summary>
		public DeviceType type { get; protected set; }

		/// <summary>
		/// Known/Common device hardware detection
		/// </summary>
		public DeviceHardware hardware { get; protected set; }

		/// <summary>
		/// Is true if device connected
		/// </summary>
		public bool connected { get; private set; }

		/// <summary>
		/// Called when device disconnects
		/// </summary>
		public event DisconnectedCallbackMethod DisconnectedCallback;
		public delegate void DisconnectedCallbackMethod(DeviceBase device);

		/// <summary>
		/// Called when device abstractions are ready for updating
		/// </summary>
		public event UpdateAbstractionCallbackMethod UpdateAbstractionCallback;
		public delegate void UpdateAbstractionCallbackMethod();

		/// <summary>
		/// All buttons this device supports
		/// </summary>
		public ReadOnlyArray<Button> buttons { get; protected set; }
		internal Button[] buttons_backing;

		/// <summary>
		/// All 1D axis/triggers/etc this device supports
		/// </summary>
		public ReadOnlyArray<Axis1D> axes1D { get; protected set; }
		internal Axis1D[] axes1D_backing;

		/// <summary>
		/// All 2D axis/joystick/etc this device supports
		/// </summary>
		public ReadOnlyArray<Axis2D> axes2D { get; protected set; }
		internal Axis2D[] axes2D_backing;

		/// <summary>
		/// All 3D axis etc this device supports
		/// </summary>
		public ReadOnlyArray<Axis3D> axes3D { get; protected set; }
		internal Axis3D[] axes3D_backing;

		/// <summary>
		/// All sliders this device supports
		/// </summary>
		public ReadOnlyArray<Slider> sliders { get; protected set; }
		internal Slider[] sliders_backing;

		/// <summary>
		/// All POV directions this device supports
		/// </summary>
		public ReadOnlyArray<uint> povDirections { get; protected set; }
		protected internal uint[] povDirections_backing;

		/// <summary>
		/// Physical count without any virtually mapped types
		/// </summary>
		public int physicalButtonCount { get; protected set; }

		/// <summary>
		/// Physical count without any virtually mapped types
		/// </summary>
		public int physicalAxis1DCount { get; protected set; }

		/// <summary>
		/// Physical count without any virtually mapped types
		/// </summary>
		public int physicalAxis2DCount { get; protected set; }

		/// <summary>
		/// Physical count without any virtually mapped types
		/// </summary>
		public int physicalAxis3DCount { get; protected set; }

		/// <summary>
		/// Physical count without any virtually mapped types
		/// </summary>
		public int physicalSliderCount { get; protected set; }

		public DeviceBase(InstanceBase instance)
		{
			this.instance = instance;
		}

		/// <summary>
		/// Create physical buttons, axes, etc
		/// </summary>
		protected void CreatePhysicalObjects(int buttonCount, int axis1DCount, int axis2DCount, int axis3DCount, int sliderCount, int povCount)
		{
			buttons = new ReadOnlyArray<Button>(buttonCount, out buttons_backing);
			for (int i = 0; i != buttonCount; ++i) buttons_backing[i] = new Button();

			axes1D = new ReadOnlyArray<Axis1D>(axis1DCount, out axes1D_backing);
			for (int i = 0; i != axis1DCount; ++i) axes1D_backing[i] = new Axis1D(Axis1DUpdateMode.Bidirectional);

			axes2D = new ReadOnlyArray<Axis2D>(axis2DCount, out axes2D_backing);
			for (int i = 0; i != axis2DCount; ++i) axes2D_backing[i] = new Axis2D();

			axes3D = new ReadOnlyArray<Axis3D>(axis3DCount, out axes3D_backing);
			for (int i = 0; i != axis3DCount; ++i) axes3D_backing[i] = new Axis3D();

			sliders = new ReadOnlyArray<Slider>(sliderCount, out sliders_backing);
			for (int i = 0; i != sliderCount; ++i) sliders_backing[i] = new Slider();

			povDirections = new ReadOnlyArray<uint>(povCount, out povDirections_backing);
			for (int i = 0; i != povCount; ++i) povDirections_backing[i] = uint.MaxValue;
		}

		protected internal virtual void Dispose()
		{
			if (connected && DisconnectedCallback != null) DisconnectedCallback(this);
			connected = false;
			DisconnectedCallback = null;
			UpdateAbstractionCallback = null;
		}

		public virtual void Update()
		{
			if (UpdateAbstractionCallback != null) UpdateAbstractionCallback();
		}

		protected void UpdateStart(bool connected)
		{
			if (!connected) UpdateDisconnected();
			if (connected != this.connected)
			{
				if (connected) RefreshDeviceInfo();
				else if (!connected && DisconnectedCallback != null) DisconnectedCallback(this);
			}
			this.connected = connected;
		}

		protected abstract void RefreshDeviceInfo();

		/// <summary>
		/// Set state back to default
		/// </summary>
		protected void UpdateDisconnected()
		{
			foreach (var button in buttons) button.Update(false);
			foreach (var axis in axes1D) axis.Update(0);
			foreach (var axis in axes2D) axis.Update(Vec2.zero);
			foreach (var axis in axes3D) axis.Update(Vec3.zero);
			foreach (var slider in sliders) slider.Update(0);
		}

		/// <summary>
		/// Sets all motors rumble on supported controllers
		/// </summary>
		/// <param name="value">0-1 min-max value</param>
		public virtual void SetRumble(float value)
		{
			// do nothing if not supported...
		}

		/// <summary>
		/// Sets motor rumble on supported controllers
		/// </summary>
		/// <param name="leftValue">0-1 min-max value on left motor</param>
		/// <param name="rightValue">0-1 min-max value on right motor</param>
		public virtual void SetRumble(float leftValue, float rightValue)
		{
			// do nothing if not supported...
		}

		/// <summary>
		/// Sets motor rumble on supported controllers
		/// </summary>
		/// <param name="value">0-1 min-max value</param>
		/// <param name="motorIndex">Motor index</param>
		public virtual void SetRumble(float value, int motorIndex)
		{
			// do nothing if not supported...
		}

		public Button GetOrCreate_Button(int? index)
		{
			if (index == null || index < 0 || index >= buttons_backing.Length) return new Button();
			return buttons_backing[(int)index];
		}

		public Axis1D GetOrCreate_Axis1D(int? index, Axis1DUpdateMode mode)
		{
			if (index == null || index < 0 || index >= axes1D_backing.Length) return new Axis1D(mode);
			return axes1D_backing[(int)index];
		}

		public Axis2D GetOrCreate_Axis2D(int? index)
		{
			if (index == null || index < 0 || index >= axes2D_backing.Length) return new Axis2D();
			return axes2D_backing[(int)index];
		}

		public Axis3D GetOrCreate_Axis3D(int? index)
		{
			if (index == null || index < 0 || index >= axes3D_backing.Length) return new Axis3D();
			return axes3D_backing[(int)index];
		}

		public Slider GetOrCreate_Slider(int? index)
		{
			if (index == null || index < 0 || index >= sliders_backing.Length) return new Slider();
			return sliders_backing[(int)index];
		}

		public DeviceMap_Axis1D CreateAxis1DMap(DeviceMapConfig_Axis1D config, Axis1D axis)
		{
			var result = new DeviceMap_Axis1D();
			result.invertSrc = config.invertSrc;
			result.axisSrc = axes1D_backing[config.axis1D_Src];
			result.axisDst = axis;
			return result;
		}

		public DeviceMap_Axis2D CreateAxis2DMap(DeviceMapConfig_Axis2D config, Axis2D axis)
		{
			var result = new DeviceMap_Axis2D();
			result.invertAxisX = config.invertAxisX;
			result.invertAxisY = config.invertAxisY;
			result.axisX_Src = axes1D_backing[config.axis1D_X_Src];
			result.axisY_Src = axes1D_backing[config.axis1D_Y_Src];
			result.axisDst = axis;
			return result;
		}

		public DeviceMap_Axis3D CreateAxis3DMap(DeviceMapConfig_Axis3D config, Axis3D axis)
		{
			var result = new DeviceMap_Axis3D();
			result.invertAxisX = config.invertAxisX;
			result.invertAxisY = config.invertAxisY;
			result.invertAxisZ = config.invertAxisZ;
			result.axisX_Src = axes1D_backing[config.axis1D_X_Src];
			result.axisY_Src = axes1D_backing[config.axis1D_Y_Src];
			result.axisZ_Src = axes1D_backing[config.axis1D_Z_Src];
			result.axisDst = axis;
			return result;
		}
	}
}
