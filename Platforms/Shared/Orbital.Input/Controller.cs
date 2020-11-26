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
		public Button button1 { get { return button1_field; } }
		protected Button button1_field;

		/// <summary>
		/// Common button: B (XB), O (PS), A {Nintendo), etc
		/// </summary>
		public Button button2 { get { return button2_field; } }
		protected Button button2_field;

		/// <summary>
		/// Common button: X (XB), □ (PS), Y {Nintendo), etc
		/// </summary>
		public Button button3 { get { return button3_field; } }
		protected Button button3_field;

		/// <summary>
		/// Common button: Y (XB), △ (PS), X {Nintendo), etc
		/// </summary>
		public Button button4 { get { return button4_field; } }
		protected Button button4_field;

		/// <summary>
		/// Common button: DPad Left
		/// </summary>
		public Button dpadLeft { get { return dpadLeft_field; } }
		protected Button dpadLeft_field;

		/// <summary>
		/// Common button: DPad Right
		/// </summary>
		public Button dpadRight { get { return dpadRight_field; } }
		protected Button dpadRight_field;

		/// <summary>
		/// Common button: DPad Down
		/// </summary>
		public Button dpadDown { get { return dpadDown_field; } }
		protected Button dpadDown_field;

		/// <summary>
		/// Common button: DPad Up
		/// </summary>
		public Button dpadUp { get { return dpadUp_field; } }
		protected Button dpadUp_field;

		/// <summary>
		/// Common button: Menu, Start, Options, etc
		/// </summary>
		public Button menu { get { return menu_field; } }
		protected Button menu_field;

		/// <summary>
		/// Common button: Back, Select, etc
		/// </summary>
		public Button back { get { return back_field; } }
		protected Button back_field;

		/// <summary>
		/// Common button: Bumper Left
		/// </summary>
		public Button bumperLeft { get { return bumperLeft_field; } }
		protected Button bumperLeft_field;

		/// <summary>
		/// Common button: Bumper Right
		/// </summary>
		public Button bumperRight { get { return bumperRight_field; } }
		protected Button bumperRight_field;

		/// <summary>
		/// Common button: Trigger Button Left
		/// </summary>
		public Button triggerButtonLeft { get { return triggerButtonLeft_field; } }
		protected Button triggerButtonLeft_field;

		/// <summary>
		/// Common button: Trigger Button Right
		/// </summary>
		public Button triggerButtonRight { get { return triggerButtonRight_field; } }
		protected Button triggerButtonRight_field;

		/// <summary>
		/// Common button: Joystick Left
		/// </summary>
		public Button joysticButtonkLeft { get { return joysticButtonkLeft_field; } }
		protected Button joysticButtonkLeft_field;

		/// <summary>
		/// Common button: Joystick Right
		/// </summary>
		public Button joystickButtonRight { get { return joystickButtonRight_field; } }
		protected Button joystickButtonRight_field;
		#endregion

		#region Common 1D Analogs
		/// <summary>
		/// Common analog: Trigger Left
		/// </summary>
		public Analog1D triggerLeft { get { return triggerLeft_field; } }
		protected Analog1D triggerLeft_field;

		/// <summary>
		/// Common analog: Trigger Right
		/// </summary>
		public Analog1D triggerRight { get { return triggerRight_field; } }
		protected Analog1D triggerRight_field;
		#endregion

		#region Common 2D Analogs
		/// <summary>
		/// Common analog: Joystick Left
		/// </summary>
		public Analog2D joystickLeft { get { return joystickLeft_field; } }
		protected Analog2D joystickLeft_field;

		/// <summary>
		/// Common analog: Joystick Right
		/// </summary>
		public Analog2D joystickRight { get { return joystickRight_field; } }
		protected Analog2D joystickRight_field;
		#endregion

		public ControllerBase(int buttonCount, int analog1DCount, int analog2DCount)
		{
			buttons = new Button[buttonCount];
			analogs_1D = new Analog1D[analog1DCount];
			analogs_2D = new Analog2D[analog2DCount];
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
			button1_field.Update(false);
			button2_field.Update(false);
			button3_field.Update(false);
			button4_field.Update(false);
			dpadLeft_field.Update(false);
			dpadRight_field.Update(false);
			dpadDown_field.Update(false);
			dpadUp_field.Update(false);
			menu_field.Update(false);
			back_field.Update(false);
			bumperLeft_field.Update(false);
			bumperRight_field.Update(false);
			triggerButtonLeft_field.Update(false);
			triggerButtonRight_field.Update(false);
			joysticButtonkLeft_field.Update(false);
			joystickButtonRight_field.Update(false);
			triggerLeft_field.Update(0, 0);
			triggerRight_field.Update(0, 0);
			joystickLeft_field.Update(Vec2.zero, 0);
			joystickRight_field.Update(Vec2.zero, 0);
		}
	}
}
