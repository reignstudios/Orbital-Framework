using Orbital.Host;
using Orbital.Host.GTK4;

namespace Orbital.Demo.GTK4
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            Application.Init("org.ReignStudios.Orbital");
            var window = new Window("Demo: GTK4", 320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}