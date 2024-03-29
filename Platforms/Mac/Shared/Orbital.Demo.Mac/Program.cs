﻿using System;
using Orbital.Host;
using Orbital.Host.Mac;

namespace Orbital.Demo.Microsoft
{
    static class Program
    {
        private static void Main(string[] args)
        {
            // init app and window
            Application.Init();
            var window = new Window(320, 240, WindowType.Tool, WindowStartupPosition.CenterScreen);
            window.SetTitle("Demo: Mac");
            window.Show();
            
            // run app till quit
            Application.Run(window);
            Application.Shutdown();
        }
    }
}