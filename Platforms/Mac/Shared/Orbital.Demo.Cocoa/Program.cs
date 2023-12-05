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
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
            window.SetTitle("Demo: Cocoa");
            window.Show();

            var display = Displays.GetPrimaryDisplay();
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}