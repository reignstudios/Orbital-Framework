namespace Orbital.Host.WPF
{
	public static class Application
	{
		public static System.Windows.Application application { get; private set; }

		public static void Init()
		{
			application = new System.Windows.Application();
		}

		public static void Shutdown()
		{
			application = null;
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
