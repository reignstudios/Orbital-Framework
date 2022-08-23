using App = System.Windows.Forms.Application;

namespace Orbital.Host.WinForms
{
	public sealed class Application : ApplicationBase
	{
		public override void Run()
		{
			App.Run();
		}

		public override void Run(WindowBase window)
		{
			var windowAbstraction = (Window)window;
			App.Run(windowAbstraction.form);
		}

		public override void RunEvents()
		{
			App.DoEvents();
		}

		public override void Exit()
		{
			App.Exit();
		}
	}
}
