using Orbital.Host;
using Orbital.Host.Wayland;

namespace Orbital.Demo.Wayland
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            Application.Init("org.ReignStudios.Orbital");
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
            window.SetTitle("Demo: Wayland");
            window.Show();
            
            // run app till quit
            //Application.Run(window);
            Application.Run();
            Application.Shutdown();
        }
    }
}