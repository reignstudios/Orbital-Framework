using System.Runtime.InteropServices;

namespace Orbital.Host.Wayland
{
	public unsafe static partial class Wayland
	{
		public struct wl_registry {}

		[StructLayout(LayoutKind.Sequential)]
		public struct wl_registry_listener
		{
			public IntPtr global;
			public IntPtr global_remove;
		}
		
		public const int WL_DISPLAY_GET_REGISTRY = 1;
		private static wl_interface wl_registry_interface;
		
		public static wl_registry* wl_display_get_registry(wl_display *wl_display)
		{
			wl_proxy *registry;
			fixed (wl_interface* wl_registry_interfacePtr = &wl_registry_interface)
			{
				registry = wl_proxy_marshal_flags((wl_proxy*)wl_display, WL_DISPLAY_GET_REGISTRY, wl_registry_interfacePtr, wl_proxy_get_version((wl_proxy*)wl_display), 0, __arglist(null));
			}
			return (wl_registry*)registry;
		}
		
		public static int wl_registry_add_listener(wl_registry *wl_registry, wl_registry_listener *listener, void *data)
		{
			return wl_proxy_add_listener((wl_proxy *) wl_registry, listener, data);
		}
	}
}
