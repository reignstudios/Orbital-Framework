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
	}
}
