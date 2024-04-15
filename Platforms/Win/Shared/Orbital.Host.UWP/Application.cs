namespace Orbital.Host.UWP
{
	public static partial class Application
	{
		public static App app { get; private set; }
		public static Window window { get; internal set; }

		internal static int preferredWidth, preferredHeight;
		internal static bool fullscreen;

		public static void Init(int preferredWidth, int preferredHeight, bool fullscreen)
		{
			Application.preferredWidth = preferredWidth;
			Application.preferredHeight = preferredHeight;
			Application.fullscreen = fullscreen;

			Windows.UI.Xaml.Application.Start(StartAppCallback);
		}

		private static void StartAppCallback(Windows.UI.Xaml.ApplicationInitializationCallbackParams p)
		{
			app = new App();
		}

		public static void Exit()
		{
			app.Exit();
		}
	}
}
