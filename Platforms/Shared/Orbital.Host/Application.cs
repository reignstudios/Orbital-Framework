using System;

namespace Orbital.Host
{
	public abstract class ApplicationBase : IDisposable
	{
		public abstract void Dispose();
		
		public abstract void Run();
		public abstract void Run(WindowBase window);
		public abstract void RunEvents();
		public abstract void Exit();
	}
}
