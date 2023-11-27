using Orbital.Host;
using Orbital.Host.Cocoa;

namespace Orbital.Demo.Cocoa
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            var application = new Application();
            var window = new Window(320, 240, WindowType.Standard, WindowStartupPosition.CenterScreen);
            window.SetTitle("Demo: Mac");
            window.Show();
            
            // run app till quit
            application.Run(window);
            //application.Run();
            application.Dispose();
        }
    }
}