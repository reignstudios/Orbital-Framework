using Orbital.Numerics;
using System;

namespace Orbital.Input
{
	public abstract class DeviceBase : IDisposable
	{
		public InstanceBase instance { get; private set; }

		/// <summary>
		/// Is true if controller connected
		/// </summary>
		public bool connected { get; private set; }

		/// <summary>
		/// Called when device disconnects
		/// </summary>
		public event DisconnectedCallbackMethod DisconnectedCallback;
		public delegate void DisconnectedCallbackMethod(DeviceBase controller);

		/// <summary>
		/// All buttons this controller supports
		/// </summary>
		public Button[] buttons { get; protected set; }

		/// <summary>
		/// All 1D analog/triggers/etc this controller supports
		/// </summary>
		public Analog1D[] analogs_1D { get; protected set; }

		/// <summary>
		/// All 2D analog/joystick/etc this controller supports
		/// </summary>
		public Analog2D[] analogs_2D { get; protected set; }

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
		/// Create interal arrays of attached buttons, analogs, etc
		/// </summary>
		protected void CreateAttachedArrays(int buttonCount, int analog1DCount, int analog2DCount)
		{
			buttons = new Button[buttonCount];
			for (int i = 0; i != buttonCount; ++i) buttons[i] = new Button(true);

			analogs_1D = new Analog1D[analog1DCount];
			for (int i = 0; i != analog1DCount; ++i) analogs_1D[i] = new Analog1D(true);

			analogs_2D = new Analog2D[analog2DCount];
			for (int i = 0; i != analog2DCount; ++i) analogs_2D[i] = new Analog2D(true);
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
			if (triggerLeft == null) triggerLeft = new Analog1D(false);
			if (triggerRight == null) triggerRight = new Analog1D(false);

			// joysticks
			if (joystickLeft == null) joystickLeft = new Analog2D(false);
			if (joystickRight == null) joystickRight = new Analog2D(false);
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
