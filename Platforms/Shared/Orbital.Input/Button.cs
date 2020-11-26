namespace Orbital.Input
{
	public struct Button
	{
		public bool on { get; private set; }
		public bool down { get; private set; }
		public bool up { get; private set; }

		internal void Update(bool on)
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
