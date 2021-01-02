using Orbital.Numerics;
using Orbital.Primitives;
using System;
using System.Collections.Generic;

namespace Orbital.Input
{
	public enum DeviceType
	{
		Unknown,
		Gamepad,
		ArcadeStick,
		FlightStick,
		SteeringWheel,
		Keyboard,
		Mouse
	}

	public abstract class DeviceBase : IDisposable
	{
		public InstanceBase instance { get; private set; }

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
		/// All buttons this device supports
		/// </summary>
		public ReadOnlyList<Button> buttons { get; protected set; }
		protected List<Button> buttons_backing;

		/// <summary>
		/// All 1D analog/triggers/etc this device supports
		/// </summary>
		public ReadOnlyList<Analog1D> analogs_1D { get; protected set; }
		protected List<Analog1D> analogs_1D_backing;

		/// <summary>
		/// All 2D analog/joystick/etc this device supports
		/// </summary>
		public ReadOnlyList<Analog2D> analogs_2D { get; protected set; }
		protected List<Analog2D> analogs_2D_backing;

		/// <summary>
		/// All 3D analog etc this device supports
		/// </summary>
		public ReadOnlyList<Analog3D> analogs_3D { get; protected set; }
		protected List<Analog3D> analogs_3D_backing;

		/// <summary>
		/// All sliders this device supports
		/// </summary>
		public ReadOnlyList<Slider> sliders { get; protected set; }
		protected List<Slider> sliders_backing;

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
		/// Common button: 3 (Sega, MS), etc
		/// </summary>
		public Button button6 { get; protected set; }

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

		#region Common 1D Analogs
		/// <summary>
		/// Common analog: Trigger Left
		/// </summary>
		public Analog1D triggerLeft { get; protected set; }

		/// <summary>
		/// Common analog: Trigger Right
		/// </summary>
		public Analog1D triggerRight { get; protected set; }
		#endregion

		#region Common 2D Analogs
		/// <summary>
		/// Common analog: Joystick Left
		/// </summary>
		public Analog2D joystickLeft { get; protected set; }

		/// <summary>
		/// Common analog: Joystick Right
		/// </summary>
		public Analog2D joystickRight { get; protected set; }
		#endregion

		public DeviceBase(InstanceBase instance)
		{
			this.instance = instance;
		}

		/// <summary>
		/// Create physical buttons, analogs, etc
		/// </summary>
		protected void CreatePhysicalObjects(int buttonCount, int analog1DCount, int analog2DCount, int analog3DCount, int sliderCount)
		{
			buttons = ReadOnlyList<Button>.Create(out buttons_backing);
			for (int i = 0; i != buttonCount; ++i) buttons_backing.Add(new Button(true));

			analogs_1D = ReadOnlyList<Analog1D>.Create(out analogs_1D_backing);
			for (int i = 0; i != analog1DCount; ++i) analogs_1D_backing.Add(new Analog1D(true, Analog1DUpdateMode.Bidirectional));

			analogs_2D = ReadOnlyList<Analog2D>.Create(out analogs_2D_backing);
			for (int i = 0; i != analog2DCount; ++i) analogs_2D_backing.Add(new Analog2D(true));

			analogs_3D = ReadOnlyList<Analog3D>.Create(out analogs_3D_backing);
			for (int i = 0; i != analog3DCount; ++i) analogs_3D_backing.Add(new Analog3D(true));

			sliders = ReadOnlyList<Slider>.Create(out sliders_backing);
			for (int i = 0; i != sliderCount; ++i) sliders_backing.Add(new Slider(true));
		}

		/// <summary>
		/// If any buttons, analogs, etc are null an instance will be created to avoid null-refs when used
		/// </summary>
		protected void CreateMissingObjects()
		{
			// primary buttons
			if (button1 == null) button1 = new Button(false);
			if (button2 == null) button2 = new Button(false);
			if (button3 == null) button3 = new Button(false);
			if (button4 == null) button4 = new Button(false);
			if (button5 == null) button5 = new Button(false);
			if (button6 == null) button6 = new Button(false);

			// dpad
			if (dpadLeft == null) dpadLeft = new Button(false);
			if (dpadRight == null) dpadRight = new Button(false);
			if (dpadDown == null) dpadDown = new Button(false);
			if (dpadUp == null) dpadUp = new Button(false);

			// options
			if (menu == null) menu = new Button(false);
			if (back == null) back = new Button(false);

			// bumbers
			if (bumperLeft == null) bumperLeft = new Button(false);
			if (bumperRight == null) bumperRight = new Button(false);

			// trigger buttons
			if (triggerButtonLeft == null) triggerButtonLeft = new Button(false);
			if (triggerButtonRight == null) triggerButtonRight = new Button(false);

			// joystick buttons
			if (joystickButtonLeft == null) joystickButtonLeft = new Button(false);
			if (joystickButtonRight == null) joystickButtonRight = new Button(false);

			// triggers
			if (triggerLeft == null) triggerLeft = new Analog1D(false, Analog1DUpdateMode.Positive);
			if (triggerRight == null) triggerRight = new Analog1D(false, Analog1DUpdateMode.Positive);

			// joysticks
			if (joystickLeft == null) joystickLeft = new Analog2D(false);
			if (joystickRight == null) joystickRight = new Analog2D(false);
		}

		/// <summary>
		/// Add object if it doesn't exist
		/// </summary>
		protected void AddVirtualObject(Button button)
		{
			if (button != null && !buttons_backing.Contains(button)) buttons_backing.Add(button);
		}

		/// <summary>
		/// Add object if it doesn't exist
		/// </summary>
		protected void AddVirtualObject(Analog1D analog)
		{
			if (analog != null && !analogs_1D_backing.Contains(analog)) analogs_1D_backing.Add(analog);
		}

		/// <summary>
		/// Add object if it doesn't exist
		/// </summary>
		protected void AddVirtualObject(Analog2D analog)
		{
			if (analog != null && !analogs_2D_backing.Contains(analog)) analogs_2D_backing.Add(analog);
		}

		/// <summary>
		/// Add object if it doesn't exist
		/// </summary>
		protected void AddVirtualObject(Analog3D analog)
		{
			if (analog != null && !analogs_3D_backing.Contains(analog)) analogs_3D_backing.Add(analog);
		}

		/// <summary>
		/// Add object if it doesn't exist
		/// </summary>
		protected void AddVirtualObject(Slider slider)
		{
			if (slider != null && !sliders_backing.Contains(slider)) sliders_backing.Add(slider);
		}

		public abstract void Dispose();
		public abstract void Update();

		protected void Update(bool connected)
		{
			if (!connected) UpdateDisconnected();
			if (connected != this.connected)
			{
				if (!connected && DisconnectedCallback != null) DisconnectedCallback(this);
			}
			this.connected = connected;
		}

		/// <summary>
		/// Set state back to default
		/// </summary>
		protected void UpdateDisconnected()
		{
			// primary buttons
			button1.Update(false);
			button2.Update(false);
			button3.Update(false);
			button4.Update(false);
			button5.Update(false);
			button6.Update(false);

			// dpad
			dpadLeft.Update(false);
			dpadRight.Update(false);
			dpadDown.Update(false);
			dpadUp.Update(false);

			// options
			menu.Update(false);
			back.Update(false);

			// bumbers
			bumperLeft.Update(false);
			bumperRight.Update(false);

			// trigger buttons
			triggerButtonLeft.Update(false);
			triggerButtonRight.Update(false);

			// joystick buttons
			joystickButtonLeft.Update(false);
			joystickButtonRight.Update(false);

			// triggers
			triggerLeft.Update(0);
			triggerRight.Update(0);

			// joysticks
			joystickLeft.Update(Vec2.zero);
			joystickRight.Update(Vec2.zero);
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
	}
}
