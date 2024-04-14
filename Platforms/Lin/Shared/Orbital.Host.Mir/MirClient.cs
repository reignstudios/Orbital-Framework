using System.Runtime.InteropServices;
using System.Security;

using BOOL = System.Int32;
using size_t = System.IntPtr;

using MirConnection = System.IntPtr;
using MirDisplayConfig = System.IntPtr;
using MirOutput = System.IntPtr;
using MirOutputMode = System.IntPtr;
using MirWindowSpec = System.IntPtr;
using MirWindow = System.IntPtr;
using MirBufferStream = System.IntPtr;
using MirWaitHandle = System.IntPtr;
using MirEvent = System.IntPtr;

using MirWindowEventCallback = System.IntPtr;

namespace Orbital.Host.Mir
{
	public unsafe static class MirClient
	{
		public const string lib = "libmirclient.so";

		public enum MirOutputConnectionState
		{
			mir_output_connection_state_disconnected = 0,
			mir_output_connection_state_connected,
			mir_output_connection_state_unknown
		}

		public enum MirPixelFormat
		{
			mir_pixel_format_invalid = 0,
			mir_pixel_format_abgr_8888 = 1,
			mir_pixel_format_xbgr_8888 = 2,
			mir_pixel_format_argb_8888 = 3,
			mir_pixel_format_xrgb_8888 = 4,
			mir_pixel_format_bgr_888 = 5,
			mir_pixel_format_rgb_888 = 6,
			mir_pixel_format_rgb_565 = 7,
			mir_pixel_format_rgba_5551 = 8,
			mir_pixel_format_rgba_4444 = 9,
			mir_pixel_formats /* Note: This is always max format + 1 */
		}

		public enum MirBufferUsage
		{
			mir_buffer_usage_hardware = 1,
			mir_buffer_usage_software
		}

		public enum MirEventType
		{
			mir_event_type_key,//MIR_DEPRECATED_ENUM(mir_event_type_key, "mir_event_type_input"),     // UNUSED since Mir 0.26
			mir_event_type_motion,//MIR_DEPRECATED_ENUM(mir_event_type_motion, "mir_event_type_input"),  // UNUSED since Mir 0.26
			mir_event_type_surface,//MIR_DEPRECATED_ENUM(mir_event_type_surface, "mir_event_type_window"),
			mir_event_type_window = mir_event_type_surface,
			mir_event_type_resize,
			mir_event_type_prompt_session_state_change,
			mir_event_type_orientation,
			mir_event_type_close_surface,//MIR_DEPRECATED_ENUM(mir_event_type_close_surface, "mir_event_type_close_window"),
			mir_event_type_close_window = mir_event_type_close_surface,
			/* Type for new style input event will be returned from mir_event_get_type
			when old style event type was mir_event_type_key or mir_event_type_motion */
			mir_event_type_input,
			mir_event_type_keymap,
			mir_event_type_input_configuration,//MIR_DEPRECATED_ENUM(mir_event_type_input_configuration, "mir_connection_set_input_config_change_callback and mir_event_type_input_device_state"),
			mir_event_type_surface_output,//MIR_DEPRECATED_ENUM(mir_event_type_surface_output, "mir_event_type_window_output"),
			mir_event_type_window_output = mir_event_type_surface_output,
			mir_event_type_input_device_state,
			mir_event_type_surface_placement,//MIR_DEPRECATED_ENUM(mir_event_type_surface_placement, "mir_event_type_window_placement"),
			mir_event_type_window_placement = mir_event_type_surface_placement,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MirGraphicsRegion
		{
			public int width;
			public int height;
			public int stride;
			public MirPixelFormat pixel_format;
			public byte* vaddr;
		}

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_connect_sync", ExactSpelling = true)]
		public static extern MirConnection mir_connect_sync(byte* server, byte* app_name);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_connection_release", ExactSpelling = true)]
		public static extern void mir_connection_release(MirConnection connection);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_connection_is_valid", ExactSpelling = true)]
		public static extern BOOL mir_connection_is_valid(MirConnection connection);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_connection_is_valid", ExactSpelling = true)]
		public static extern byte* mir_connection_get_error_message(MirConnection connection);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_connection_create_display_configuration", ExactSpelling = true)]
		public static extern MirDisplayConfig mir_connection_create_display_configuration(MirConnection connection);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_display_config_get_num_outputs", ExactSpelling = true)]
		public static extern int mir_display_config_get_num_outputs(MirDisplayConfig config);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_display_config_get_output", ExactSpelling = true)]
		public static extern MirOutput mir_display_config_get_output(MirDisplayConfig config, size_t index);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_get_connection_state", ExactSpelling = true)]
		public static extern MirOutputConnectionState mir_output_get_connection_state(MirOutput output);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_is_enabled", ExactSpelling = true)]
		public static extern BOOL mir_output_is_enabled(MirOutput output);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_get_num_pixel_formats", ExactSpelling = true)]
		public static extern int mir_output_get_num_pixel_formats(MirOutput output);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_get_pixel_format", ExactSpelling = true)]
		public static extern MirPixelFormat mir_output_get_pixel_format(MirOutput output, size_t index);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_get_current_mode", ExactSpelling = true)]
		public static extern MirOutputMode mir_output_get_current_mode(MirOutput output);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_display_config_release", ExactSpelling = true)]
		public static extern void mir_display_config_release(MirDisplayConfig config);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_mode_get_width", ExactSpelling = true)]
		public static extern int mir_output_mode_get_width(MirOutputMode mode);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_mode_get_height", ExactSpelling = true)]
		public static extern int mir_output_mode_get_height(MirOutputMode mode);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_output_mode_get_refresh_rate", ExactSpelling = true)]
		public static extern double mir_output_mode_get_refresh_rate(MirOutputMode mode);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_create_normal_window_spec", ExactSpelling = true)]
		public static extern MirWindowSpec mir_create_normal_window_spec(MirConnection connection, int width, int height);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_spec_set_pixel_format", ExactSpelling = true)]
		public static extern void mir_window_spec_set_pixel_format(MirWindowSpec spec, MirPixelFormat format);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_spec_set_name", ExactSpelling = true)]
		public static extern void mir_window_spec_set_name(MirWindowSpec spec, byte* name);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_spec_set_buffer_usage", ExactSpelling = true)]
		public static extern void mir_window_spec_set_buffer_usage(MirWindowSpec spec, MirBufferUsage usage);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_create_window_sync", ExactSpelling = true)]
		public static extern MirWindow mir_create_window_sync(MirWindowSpec requested_specification);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_spec_release", ExactSpelling = true)]
		public static extern void mir_window_spec_release(MirWindowSpec spec);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_get_buffer_stream", ExactSpelling = true)]
		public static extern MirBufferStream mir_window_get_buffer_stream(MirWindow window);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_buffer_stream_set_swapinterval", ExactSpelling = true)]
		public static extern MirWaitHandle mir_buffer_stream_set_swapinterval(MirBufferStream stream, int interval);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_set_event_handler", ExactSpelling = true)]
		public static extern void mir_window_set_event_handler(MirWindow window, MirWindowEventCallback callback, void* context);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_window_release_sync", ExactSpelling = true)]
		public static extern void mir_window_release_sync(MirWindow window);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_buffer_stream_get_graphics_region", ExactSpelling = true)]
		public static extern BOOL mir_buffer_stream_get_graphics_region(MirBufferStream buffer_stream, MirGraphicsRegion* graphics_region);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_buffer_stream_swap_buffers_sync", ExactSpelling = true)]
		public static extern void mir_buffer_stream_swap_buffers_sync(MirBufferStream buffer_stream);

		[SuppressUnmanagedCodeSecurity]
		[DllImport(lib, EntryPoint = "mir_event_get_type", ExactSpelling = true)]
		public static extern MirEventType mir_event_get_type(MirEvent e);
	}
}
