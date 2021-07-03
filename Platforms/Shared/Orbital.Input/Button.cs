namespace Orbital.Input
{
	public class Button
	{
		/// <summary>
		/// Button actively being pressed
		/// </summary>
		public bool on { get; private set; }

		/// <summary>
		/// Button was pressed
		/// </summary>
		public bool down { get; private set; }

		/// <summary>
		/// Button was released
		/// </summary>
		public bool up { get; private set; }

		public void Update(bool on)
		{
			down = false;
			up = false;
			if (this.on != on)
			{
				if (on) down = true;
				else up = true;
			}
			this.on = on;
		}
	}

	struct ButtonNameMap
	{
		public Button button;
		public string name;

		public ButtonNameMap(Button button, string name)
		{
			this.button = button;
			this.name = name;
		}
	}
}
