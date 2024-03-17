using System.Runtime.InteropServices;

using uint32_t = System.UInt32;

namespace Orbital.Host.Wayland
{
	public unsafe static partial class Wayland
	{
		public const string libClient = "wayland-client.so";
		
		public struct wl_display {}
		public struct wl_proxy {}
		
		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct wl_interface
		{
			/** Interface name */
			public byte *name;
			/** Interface version */
			public int version;
			/** Number of methods (requests) */
			public int method_count;
			/** Method (request) signatures */
			public wl_message *methods;
			/** Number of events */
			public int event_count;
			/** Event signatures */
			public wl_message *events;
		}
	
		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct wl_message
		{
			/** Message name */
			public byte *name;
			/** Message signature */
			public byte *signature;
			/** Object argument interfaces */
			public wl_interface **types;
		};
		
		[DllImport(libClient, ExactSpelling = true)]
		public static extern wl_display* wl_display_connect(byte* name);
		
		[DllImport(libClient, ExactSpelling = true)]
		public static extern void wl_display_disconnect(wl_display* display);
		
		[DllImport(libClient, ExactSpelling = true)]
		public static extern wl_proxy* wl_proxy_marshal_flags(wl_proxy *proxy, uint32_t opcode, wl_interface* _interface, uint32_t version, uint32_t flags, __arglist);
		
		[DllImport(libClient, ExactSpelling = true)]
		public static extern uint32_t wl_proxy_get_version(wl_proxy *proxy);
		
		[DllImport(libClient, ExactSpelling = true)]
		public static extern int wl_proxy_add_listener(wl_proxy *proxy, void* implementation, void *data);
		
		[DllImport(libClient, ExactSpelling = true)]
		public static extern int wl_display_roundtrip(wl_display *display);
	}
}
