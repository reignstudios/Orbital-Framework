using App = System.Windows.Forms.Application;

namespace Orbital.Host.WinForms
{
	public static class Application
	{
		public static void Init()
		{
			// do nothing...
		}

		public static void Shutdown()
		{
			// do nothing...
		}

		public static void Run()
		{
			App.Run();
		}

		public static void Run(Window window)
		{
			App.Run(window.form);
		}

		public static void RunEvents()
		{
			App.DoEvents();
		}

		public static void Exit()
		{
			App.Exit();
		}
	}
}
