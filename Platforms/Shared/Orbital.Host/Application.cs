using System;

namespace Orbital.Host
{
	public abstract class ApplicationBase
	{
		public abstract void Run();
		public abstract void Run(WindowBase window);
		public abstract void RunEvents();
		public abstract void Exit();
	}
}
