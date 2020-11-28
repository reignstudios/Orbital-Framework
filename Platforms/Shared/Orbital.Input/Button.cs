namespace Orbital.Input
{
	public struct Button
	{
		public string name;
		public bool on;
		public bool down;
		public bool up;

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
