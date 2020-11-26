namespace Orbital.Input.XInput
{
	public sealed class Controller : ControllerBase
	{
		public Controller()
		: base(16, 2, 2)
		{ }

		internal void Update(bool connected, ref XINPUT_STATE state)
		{
			Update(connected);
			if (!connected) return;


		}
	}
}
