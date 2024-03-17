using System;
using System.Runtime.InteropServices;

namespace Orbital.Host.Wayland
{
	public unsafe static class Application
	{
		public static Wayland.wl_display* display { get; private set; }
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void registry_add_object_Method(void *data, Wayland.wl_registry* registry, uint name, byte* _interface, uint version);
		private static void registry_add_object(void *data, Wayland.wl_registry* registry, uint name, byte* _interface, uint version)
		{
			throw new Exception();
			return;
			/*if (!strcmp(interface,wl_compositor_interface.name))
			{
				compositor = (struct wl_compositor*)(wl_registry_bind (registry, name, &wl_compositor_interface, 1));
			}
			else if (strcmp(interface, wl_subcompositor_interface.name) == 0)
			{
				subcompositor = (struct wl_subcompositor*)(wl_registry_bind(registry, name, &wl_subcompositor_interface, 1));
			}
			else if (!strcmp(interface,wl_seat_interface.name))
			{
				seat = (struct wl_seat*)(wl_registry_bind (registry, name, &wl_seat_interface, 1));
				wl_seat_add_listener (seat, &seat_listener, data);
			}
			else if (strcmp(interface, wl_shm_interface.name) == 0)
			{
				shm = (struct wl_shm*)(wl_registry_bind(registry, name, &wl_shm_interface, 1));
				cursor_theme = wl_cursor_theme_load(NULL, 32, shm);
			}
			else if (strcmp(interface, xdg_wm_base_interface.name) == 0)
			{
				xdg_wm_base = (struct xdg_wm_base*)wl_registry_bind(registry, name, &xdg_wm_base_interface, MIN(version, 2));
			}
			else if (strcmp(interface, zxdg_decoration_manager_v1_interface.name) == 0)
			{
				decoration_manager = wl_registry_bind(registry, name, &zxdg_decoration_manager_v1_interface, 1);
			}*/
		}
		
		public static void Init()
		{
			// get display
			display = Wayland.wl_display_connect(null);
			if (display == null) throw new Exception("Cannot connect to Wayland server");
			
			// que registry
			var registry = Wayland.wl_display_get_registry(display);
			registry_add_object_Method registry_add_object_method = registry_add_object;
			var registry_listener = new Wayland.wl_registry_listener()
			{
				global = Marshal.GetFunctionPointerForDelegate(registry_add_object_method)
			};
			Wayland.wl_registry_add_listener(registry, &registry_listener, null);
			Wayland.wl_display_roundtrip(display);
		}

		public static void Shutdown()
		{
			if (display != null)
			{
				Wayland.wl_display_disconnect(display);
				display = null;
			}
		}

		public static void Run()
		{
			// TODO
		}

		public static void Run(Window window)
		{
			// TODO
		}

		public static void RunEvents()
		{
			// TODO
		}

		public static void Exit()
		{
			// TODO
		}
	}
}
