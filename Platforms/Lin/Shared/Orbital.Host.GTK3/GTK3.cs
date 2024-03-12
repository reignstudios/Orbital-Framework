using System.Runtime.InteropServices;

namespace Orbital.Host.GTK3
{
	public unsafe static class GTK3
	{
		public const string lib = "libgtk-3.so.0";

		public enum GApplicationFlags
		{
			G_APPLICATION_FLAGS_NONE = 0,
			G_APPLICATION_DEFAULT_FLAGS = 0,
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
		
		public static delegate* <void> G_CALLBACK(delegate* <IntPtr, void*, void> activate_callback)
		{
			return (delegate* <void>)activate_callback;
		}
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gtk_application_new(byte* application_id, GApplicationFlags flags);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void g_object_unref(IntPtr obj);

		[DllImport(lib, ExactSpelling = true)]
		public static extern ulong g_signal_connect_data(IntPtr instance, byte* detailed_signal, delegate* <void> c_handler, IntPtr data, IntPtr destroy_data, int connect_flags);
		public static ulong g_signal_connect(IntPtr instance, byte* detailed_signal, delegate* <void> c_handler, IntPtr data)
		{
			return g_signal_connect_data(instance, detailed_signal, c_handler, data, IntPtr.Zero, 0);
		}

		[DllImport(lib, ExactSpelling = true)]
		public static extern int g_application_run(IntPtr application, int argc, char** argv);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gtk_application_window_new(IntPtr application);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_title(IntPtr window, byte* title);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_default_size(IntPtr window, int width, int height);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_widget_show_all(IntPtr widget);
	}
}
