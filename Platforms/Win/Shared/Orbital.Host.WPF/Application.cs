namespace Orbital.Host.WPF
{
	public static class Application
	{
		public static readonly System.Windows.Application application;

		static Application()
		{
			application = new System.Windows.Application();
		}

		public static void Run()
		{
			application.Run();
		}

		public static void Run(Window window)
		{
			application.Run(window.window);
		}

		public static void Exit()
		{
			application.Shutdown();
		}
	}
}
