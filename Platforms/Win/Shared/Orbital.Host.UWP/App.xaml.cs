using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using UWPApplication = Windows.UI.Xaml.Application;
using UWPWindow = Windows.UI.Xaml.Window;

namespace Orbital.Host.UWP
{
	public sealed partial class App : UWPApplication
	{
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // check if app memory still intact
			var frame = UWPWindow.Current.Content as Frame;
            if (frame == null)
            {
                // add frame
                frame = new Frame();
                frame.NavigationFailed += OnNavigationFailed;
				UWPWindow.Current.Content = frame;

				// add Orbital Window
				Application.window = new Window(UWPWindow.Current);

				// other states
				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
                else
                {
					// set prefured settings
					var view = ApplicationView.GetForCurrentView();
                    if (Application.fullscreen)
                    {
						ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
						if (!view.IsFullScreenMode)
                        {
							view.FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;
							view.TryEnterFullScreenMode();
						}
					}
                    else
                    {
					    view.SetPreferredMinSize(new Size(100, 100));// allow small window sizes up to 100
					    ApplicationView.PreferredLaunchViewSize = new Size(Application.preferredWidth, Application.preferredHeight);
					    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
                    }
				}
            }

            // activate window
			if (!e.PrelaunchActivated)
            {
				//rootFrame.Content = new MainPage();// TODO: add rendering content here?
				UWPWindow.Current.Activate();
            }
		}

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}
	}
}
