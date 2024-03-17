using System.Runtime.InteropServices;

using GdkWindow = System.IntPtr;
using GdkDevice = System.IntPtr;
using GdkEventSequence = System.IntPtr;
using cairo_region_t = System.IntPtr;
using GdkAtom = System.IntPtr;
using GdkDragContext = System.IntPtr;
using gboolean = System.Int32;
using gchar = System.Byte;
using gint8 = System.SByte;
using guint8 = System.Byte;
using gint16 = System.Int16;
using guint16 = System.UInt16;
using gshort = System.Int16;
using gint = System.Int32;
using guint32 = System.UInt32;
using guint = System.UInt32;
using gulong = System.UInt64;
using gdouble = System.Double;
using GQuark = System.UInt32;
using gpointer = System.IntPtr;

namespace Orbital.Host.GTK4
{
	public unsafe static class GTK4
	{
		public const string lib = "libgtk-4.so";
		
		public enum GApplicationFlags
		{
			G_APPLICATION_FLAGS_NONE,
			G_APPLICATION_IS_SERVICE  =          (1 << 0),
			G_APPLICATION_IS_LAUNCHER =          (1 << 1),

			G_APPLICATION_HANDLES_OPEN =         (1 << 2),
			G_APPLICATION_HANDLES_COMMAND_LINE = (1 << 3),
			G_APPLICATION_SEND_ENVIRONMENT    =  (1 << 4),

			G_APPLICATION_NON_UNIQUE =           (1 << 5),

			G_APPLICATION_CAN_OVERRIDE_APP_ID =  (1 << 6),
			G_APPLICATION_ALLOW_REPLACEMENT   =  (1 << 7),
			G_APPLICATION_REPLACE             =  (1 << 8)
		}
		
		public enum GConnectFlags
		{
			G_CONNECT_AFTER	= 1 << 0,
			G_CONNECT_SWAPPED	= 1 << 1
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkRectangle
		{
			public int x, y;
			public int width, height;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GError
		{
			public GQuark       domain;
			public gint         code;
			public gchar       *message;
		}
		
		public static IntPtr G_CALLBACK<T>(T activate_callback) where T : Delegate
		{
			return Marshal.GetFunctionPointerForDelegate(activate_callback);
		}
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gtk_application_new(byte* application_id, GApplicationFlags flags);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_object_unref(IntPtr obj);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern ulong g_signal_connect_data(IntPtr instance, byte* detailed_signal, IntPtr c_handler, void* data, IntPtr destroy_data, GConnectFlags connect_flags);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_signal_handler_disconnect(IntPtr instance, gulong handler_id);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern int g_application_run(IntPtr application, int argc, byte** argv);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gtk_application_window_new(IntPtr application);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_title(IntPtr window, byte* title);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_default_size(IntPtr window, int width, int height);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_present(IntPtr window);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_close(IntPtr window);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_fullscreen(IntPtr window);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_resizable(IntPtr window, gboolean resizable);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_get_default_size(IntPtr window, int* width, int* height);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_decorated(IntPtr window, gboolean setting);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_destroy(IntPtr window);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_application_quit(IntPtr application);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern gboolean g_application_register(IntPtr application, IntPtr cancellable, GError **error);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_application_activate(IntPtr application);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_settings_sync();

		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr g_main_context_default();
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern gboolean g_main_context_acquire(IntPtr context);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern gboolean g_main_context_iteration(IntPtr context, gboolean may_block);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_main_context_dispatch(IntPtr context);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_main_context_release(IntPtr context);
		
		public static ulong g_signal_connect(IntPtr instance, byte* detailed_signal, IntPtr c_handler, void* data)
		{
			return g_signal_connect_data(instance, detailed_signal, c_handler, data, IntPtr.Zero, 0);
		}
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_display_get_monitors(IntPtr display);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_surface_new_toplevel(IntPtr display);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_display_get_monitor_at_surface(IntPtr display, IntPtr surface);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern guint g_list_model_get_n_items(IntPtr list);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern gpointer g_list_model_get_item(IntPtr list, guint position);

		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_display_get_default();
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gdk_monitor_get_geometry(IntPtr monitor, GdkRectangle* workarea);
	}
}
