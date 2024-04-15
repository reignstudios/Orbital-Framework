using Orbital.Host;
using Orbital.Host.API;

namespace Orbital.Demo.API
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app
            var desc = new ApplicationDesc()
            {
                supportedAPIs = ApplicationAPI.X11 | ApplicationAPI.Wayland | ApplicationAPI.GTK3 | ApplicationAPI.GTK4,
                appID = "com.Reign-Studios.Orbital"
            };
            Application.Init(desc);
            
            // init window
            var windowDesc = new WindowDesc()
            {
                title = "Demo: API: " + Application.api,
				width = 320,
                height = 240,
                type = WindowType.Tool,
                startupPosition = WindowStartupPosition.CenterScreen
            };
            var window = new Window(windowDesc);
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}