using System;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace Orbital.Host.Microsoft
{
	public static class Application
	{
		sealed class ApplicationDelegates : NSApplicationDelegate
		{
			public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
			{
				exit = true;
				return NSApplicationTerminateReply.Cancel;
			}

			public override void WillTerminate(NSNotification notification)
			{
				exit = true;
			}
			
			[Action("quitMenuCallback:")]
			public void QuitMenuCallback(NSObject sender)
			{
				exit = true;
				handle.Stop(handle);
			}
		}
		
		public static NSApplication handle { get; private set; }
		public static NSMenu menubar { get; private set; }
		private static bool exit;

		public static void Init()
		{
			NSApplication.Init();
			handle = NSApplication.SharedApplication;
			var appDelegates = new ApplicationDelegates();
			handle.Delegate = appDelegates;
			handle.ActivationPolicy = NSApplicationActivationPolicy.Regular;
			
			// add quit menu
			menubar = new NSMenu();
			var appMenuItem = new NSMenuItem();
			menubar.AddItem(appMenuItem);
			handle.MainMenu = menubar;
			var appMenu = new NSMenu();
			var quitMenuItem = new NSMenuItem("Quit", new Selector("quitMenuCallback:"), "q");
			quitMenuItem.Target = appDelegates;
			appMenu.AddItem(quitMenuItem);
			appMenuItem.Submenu = appMenu;
			
			// activate
			handle.FinishLaunching();
		}

		public static void Shutdown()
		{
			if (handle != null)
			{
				handle.Dispose();
				handle = null;
				Environment.Exit(0); // TODO: something is blocking app from fully exiting
			}
		}

		public static void Run()
		{
			handle.Run();
		}

		public static void Run(Window window)
		{
			while (!exit && !window.IsClosed())
			{
				RunEvents();
			}
		}

		public static void RunEvents()
		{
			var e = handle.NextEvent(NSEventMask.AnyEvent, NSDate.DistantFuture, NSRunLoopMode.Default, true);
			if (e != null) handle.SendEvent(e);
		}

		public static void Exit()
		{
			exit = true;
		}
	}
}
