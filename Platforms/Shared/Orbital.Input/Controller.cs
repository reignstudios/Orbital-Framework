using Orbital.Numerics;

namespace Orbital.Input
{
	public abstract class ControllerBase
	{
		/// <summary>
		/// Is true if controller connected
		/// </summary>
		public bool connected { get { return connected_State; } }
		private bool connected_State;

		/// <summary>
		/// Called when device disconnects
		/// </summary>
		public event DisconnectedCallbackMethod DisconnectedCallback;
		public delegate void DisconnectedCallbackMethod(ControllerBase controller);

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
		public Button button1;

		/// <summary>
		/// Common button: B (XB), O (PS), A {Nintendo), etc
		/// </summary>
		public Button button2;

		/// <summary>
		/// Common button: X (XB), □ (PS), Y {Nintendo), etc
		/// </summary>
		public Button button3;

		/// <summary>
		/// Common button: Y (XB), △ (PS), X {Nintendo), etc
		/// </summary>
		public Button button4;

		/// <summary>
		/// Common button: DPad Left
		/// </summary>
		public Button dpadLeft;

		/// <summary>
		/// Common button: DPad Right
		/// </summary>
		public Button dpadRight;

		/// <summary>
		/// Common button: DPad Down
		/// </summary>
		public Button dpadDown;

		/// <summary>
		/// Common button: DPad Up
		/// </summary>
		public Button dpadUp;

		/// <summary>
		/// Common button: Menu, Start, Options, etc
		/// </summary>
		public Button menu;

		/// <summary>
		/// Common button: Back, Select, etc
		/// </summary>
		public Button back;

		/// <summary>
		/// Common button: Bumper Left
		/// </summary>
		public Button bumperLeft;

		/// <summary>
		/// Common button: Bumper Right
		/// </summary>
		public Button bumperRight;

		/// <summary>
		/// Common button: Trigger Button Left
		/// </summary>
		public Button triggerButtonLeft;

		/// <summary>
		/// Common button: Trigger Button Right
		/// </summary>
		public Button triggerButtonRight;

		/// <summary>
		/// Common button: Joystick Left
		/// </summary>
		public Button joystickButtonLeft;

		/// <summary>
		/// Common button: Joystick Right
		/// </summary>
		public Button joystickButtonRight;
		#endregion

		#region Common 1D Analogs
		/// <summary>
		/// Common analog: Trigger Left
		/// </summary>
		public Analog1D triggerLeft;

		/// <summary>
		/// Common analog: Trigger Right
		/// </summary>
		public Analog1D triggerRight;
		#endregion

		#region Common 2D Analogs
		/// <summary>
		/// Common analog: Joystick Left
		/// </summary>
		public Analog2D joystickLeft;

		/// <summary>
		/// Common analog: Joystick Right
		/// </summary>
		public Analog2D joystickRight;
		#endregion

		public ControllerBase(int buttonCount, int analog1DCount, int analog2DCount)
		{
			buttons = new Button[buttonCount];
			analogs_1D = new Analog1D[analog1DCount];
			analogs_2D = new Analog2D[analog2DCount];

			const float defaultTolerance = .1f;
			triggerLeft.tolerance = defaultTolerance;
			triggerRight.tolerance = defaultTolerance;
			joystickLeft.tolerance = defaultTolerance;
			joystickRight.tolerance = defaultTolerance;
			for (int i = 0; i != analogs_1D.Length; ++i) analogs_1D[i].tolerance = defaultTolerance;
			for (int i = 0; i != analogs_2D.Length; ++i) analogs_2D[i].tolerance = defaultTolerance;
		}

		protected void Update(bool connected)
		{
			if (!connected) UpdateDisconnected();
			if (connected != connected_State)
			{
				if (!connected && DisconnectedCallback != null) DisconnectedCallback(this);
			}
			connected_State = connected;
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
		/// Copy common values into arrays (buttons, analogs_1D & analogs_2D)
		/// </summary>
		public abstract void UpdateArraysToCommon();

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
		/// <param name="leftValue">0-1 min-max value</param>
		/// <param name="rightValue">0-1 min-max value</param>
		public virtual void SetRumble(float leftValue, float rightValue)
		{
			// do nothing if not supported...
		}
	}
}
