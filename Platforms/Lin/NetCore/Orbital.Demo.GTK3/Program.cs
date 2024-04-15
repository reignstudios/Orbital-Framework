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
            var window = new Window("Demo: GTK3", 320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen, false);
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}