using Orbital.Host;
using Orbital.Host.Cocoa;

namespace Orbital.Demo.Cocoa
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            Application.Init();
            var window = new Window("Demo: Cocoa", 320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen, false);
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}