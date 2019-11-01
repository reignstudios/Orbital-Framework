namespace Orbital.Host.WinForms
{
	public sealed class Application : ApplicationBase
	{
		public override void Run()
		{
			System.Windows.Forms.Application.Run();
		}

		public override void Run(WindowBase window)
		{
			var windowAbstraction = (Window)window;
			System.Windows.Forms.Application.Run(windowAbstraction.form);
		}
	}
}
