namespace Orbital.Input
{
	public class Button
	{
		/// <summary>
		/// Name of button
		/// </summary>
		public string name;

		/// <summary>
		/// Is the button attached to a device
		/// </summary>
		public readonly bool attached;

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

		public Button(bool attached)
		{
			this.attached = attached;
		}

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
}
