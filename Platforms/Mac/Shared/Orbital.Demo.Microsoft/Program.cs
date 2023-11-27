using Orbital.Host;
using Orbital.Host.Microsoft;

namespace Orbital.Demo.Microsoft
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            var application = new Application();
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
            window.SetTitle("Demo: Xamarin");
            window.Show();
            
            // run app till quit
            application.Run();
            application.Dispose();
        }
    }
}