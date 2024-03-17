using Orbital.Host;
using Orbital.Host.GTK3;

namespace Orbital.Demo.GTK3
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            Application.Init("org.ReignStudios.Orbital");
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen, false);
            window.SetTitle("Demo: GTK3");
            window.Show();
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}