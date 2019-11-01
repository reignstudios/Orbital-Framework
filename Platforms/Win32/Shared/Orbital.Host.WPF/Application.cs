using System;

namespace Orbital.Host.WPF
{
	public sealed class Application : ApplicationBase
	{
		public readonly System.Windows.Application application;

		public Application()
		{
			application = new System.Windows.Application();
		}

		public override void Run()
		{
			application.Run();
		}

		public override void Run(WindowBase window)
		{
			var windowAbstraction = (Window)window;
			application.Run(windowAbstraction.window);
		}

		public override void RunEvents()
		{
			throw new NotSupportedException("WPF doesn't support manual event pumping");
		}

		public override void Exit()
		{
			application.Shutdown();
		}
	}
}
