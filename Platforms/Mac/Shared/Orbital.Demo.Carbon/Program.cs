using Orbital.Host;
using Orbital.Host.Carbon;

namespace Orbital.Demo.Carbon
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
            window.SetTitle("Demo: Carbon");
            window.Show();
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}