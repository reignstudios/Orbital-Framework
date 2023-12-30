﻿using Orbital.Host;
using Orbital.Host.X11;

namespace Orbital.Demo.X11
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            Application.Init();
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen, false);
            window.SetTitle("Demo: X11");
            window.Show();
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}