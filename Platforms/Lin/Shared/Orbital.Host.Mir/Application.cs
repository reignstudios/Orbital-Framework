using System;
using System.Runtime.InteropServices;
using System.Reflection;
using Orbital.OS.Lin;

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
using static Orbital.Host.Mir.MirClient;
using System.Text;

namespace Orbital.Host.Mir
{
	public unsafe static class Application
	{
		private static bool exit;

		static int BYTES_PER_PIXEL(MirClient.MirPixelFormat format)
		{
			switch (format)
			{
				case MirClient.MirPixelFormat.mir_pixel_format_abgr_8888:
				case MirClient.MirPixelFormat.mir_pixel_format_xbgr_8888:
				case MirClient.MirPixelFormat.mir_pixel_format_argb_8888:
				case MirClient.MirPixelFormat.mir_pixel_format_xrgb_8888:
					return 4;

				case MirClient.MirPixelFormat.mir_pixel_format_bgr_888:
				case MirClient.MirPixelFormat.mir_pixel_format_rgb_888:
					return 3;

				case MirClient.MirPixelFormat.mir_pixel_format_rgb_565:
				case MirClient.MirPixelFormat.mir_pixel_format_rgba_5551:
				case MirClient.MirPixelFormat.mir_pixel_format_rgba_4444:
					return 2;
			}
			return 0;
		}

		static MirOutput find_active_output(MirDisplayConfig conf)
		{
			int num_outputs = MirClient.mir_display_config_get_num_outputs(conf);
			for (int i = 0; i < num_outputs; i++)
			{
				MirOutput output = MirClient.mir_display_config_get_output(conf, (size_t)i);
				MirClient.MirOutputConnectionState state = MirClient.mir_output_get_connection_state(output);
				if (state == MirClient.MirOutputConnectionState.mir_output_connection_state_connected && MirClient.mir_output_is_enabled(output) != 0)
				{
					return output;
				}
			}

			return MirOutput.Zero;
		}

		delegate void MirWindowEventCallbackMethod(MirWindow window, MirEvent e, void* context);
		static void on_event(MirWindow window, MirEvent e, void* context)
		{
			MirClient.MirEventType event_type = MirClient.mir_event_get_type(e);
			if (event_type == MirClient.MirEventType.mir_event_type_close_window) exit = true;
		}

		public static void Init(string appID)
		{
			LibraryResolver.Init(Assembly.GetExecutingAssembly());

			// get connection
			Console.WriteLine("Calling: mir_connect_sync");
			byte[] appName = Encoding.UTF8.GetBytes("MirWinSharp\0");
			MirConnection conn;
			fixed (byte* appNamePtr = appName) conn = mir_connect_sync(null, appNamePtr);
			if (mir_connection_is_valid(conn) == 0)
			{
				byte* ePtr = mir_connection_get_error_message(conn);
				string e = Marshal.PtrToStringAnsi((IntPtr)ePtr);
				throw new Exception(string.Format("Could not connect to a display server: {0}", e));
			}

			// get display config
			Console.WriteLine("Calling: mir_connection_create_display_configuration");
			MirDisplayConfig display_config = mir_connection_create_display_configuration(conn);

			// get display output
			Console.WriteLine("Calling: find_active_output");
			MirOutput output = find_active_output(display_config);
			if (output == MirOutput.Zero)
			{
				mir_connection_release(conn);
				throw new Exception("No active outputs found.");
			}

			// validate RGBA8 format exists
			Console.WriteLine("Calling: mir_output_get_num_pixel_formats");
			MirPixelFormat pixel_format = MirPixelFormat.mir_pixel_format_invalid;
			int num_pfs = mir_output_get_num_pixel_formats(output);

			bool hasFormat_abgr = false;
			bool hasFormat_xbgr = false;
			bool hasFormat_argb = false;
			bool hasFormat_xrgb = false;
			for (int i = 0; i < num_pfs; i++)
			{
				Console.WriteLine("Calling: mir_output_get_pixel_format: " + i);
				MirPixelFormat f = mir_output_get_pixel_format(output, (size_t)i);
				if (BYTES_PER_PIXEL(f) == 4)
				{
					switch (f)
					{
						case MirPixelFormat.mir_pixel_format_abgr_8888: hasFormat_abgr = true; break;
						case MirPixelFormat.mir_pixel_format_xbgr_8888: hasFormat_xbgr = true; break;
						case MirPixelFormat.mir_pixel_format_argb_8888: hasFormat_argb = true; break;
						case MirPixelFormat.mir_pixel_format_xrgb_8888: hasFormat_xrgb = true; break;
					}
				}
			}

			bool isABGR = true;
			if (hasFormat_abgr)
			{
				pixel_format = MirPixelFormat.mir_pixel_format_abgr_8888;
				isABGR = true;
			}
			else if (hasFormat_xbgr)
			{
				pixel_format = MirPixelFormat.mir_pixel_format_xbgr_8888;
				isABGR = true;
			}
			else if (hasFormat_argb)
			{
				pixel_format = MirPixelFormat.mir_pixel_format_argb_8888;
				isABGR = false;
			}
			else if (hasFormat_xrgb)
			{
				pixel_format = MirPixelFormat.mir_pixel_format_xrgb_8888;
				isABGR = false;
			}

			if (pixel_format == MirPixelFormat.mir_pixel_format_invalid)
			{
				mir_connection_release(conn);
				throw new Exception("Could not find a fast 32-bit pixel format");
			}

			// get display size
			Console.WriteLine("Calling: mir_output_get_current_mode");
			MirOutputMode mode = mir_output_get_current_mode(output);
			int width = mir_output_mode_get_width(mode);
			int height = mir_output_mode_get_height(mode);
			mir_display_config_release(display_config);

			// create window
			Console.WriteLine("Calling: mir_create_normal_window_spec");
			MirWindowSpec spec = mir_create_normal_window_spec(conn, width, height);
			mir_window_spec_set_pixel_format(spec, pixel_format);
			byte[] windowName = Encoding.UTF8.GetBytes("Mir C#\0");
			fixed (byte* windowNamePtr = windowName) mir_window_spec_set_name(spec, windowNamePtr);
			mir_window_spec_set_buffer_usage(spec, MirBufferUsage.mir_buffer_usage_software);

			Console.WriteLine("Calling: mir_create_window_sync");
			MirWindow window = mir_create_window_sync(spec);
			mir_window_spec_release(spec);
			if (window == MirWindow.Zero)
			{
				throw new Exception("Failed to create Window");
			}

			// run
			Console.WriteLine("Calling: mir_window_get_buffer_stream");
			MirBufferStream bs = mir_window_get_buffer_stream(window);
			mir_buffer_stream_set_swapinterval(bs, 0);// TODO: should swap be set to 1 instead of 0 ??
			var on_event_callback = new MirWindowEventCallbackMethod(on_event);// we keep this in memory verbose like this so GC doesn't delete it
			var on_event_callback_ptr = Marshal.GetFunctionPointerForDelegate(on_event_callback);
			Console.WriteLine("Calling: mir_window_set_event_handler");
			mir_window_set_event_handler(window, on_event_callback_ptr, null);
			while (!exit)
			{
				// get buffer
				//Console.WriteLine ("Calling: mir_buffer_stream_get_graphics_region");
				MirGraphicsRegion backbuffer;
				mir_buffer_stream_get_graphics_region(bs, &backbuffer);

				// clear buffer
				//Console.WriteLine ("Writing buffer...");
				byte* data = backbuffer.vaddr;
				int size = backbuffer.width * backbuffer.height * 4;
				byte channel0 = (byte)(isABGR ? 255 : 0);
				byte channel2 = (byte)(isABGR ? 0 : 255);
				for (int i = 0; i < size; i += 4)
				{
					data[i + 0] = channel0;
					data[i + 1] = 0;
					data[i + 2] = channel2;// R or B
					data[i + 3] = 255;// alpha
				}

				// swap buffer
				//Console.WriteLine ("Calling: mir_buffer_stream_swap_buffers_sync");
				mir_buffer_stream_swap_buffers_sync(bs);
				//System.Threading.Thread.Sleep (1000 / 60);
			}

			// shutdown
			Console.WriteLine("Calling: Shutdown...");
			mir_window_set_event_handler(window, MirWindowEventCallback.Zero, null);
			mir_window_release_sync(window);
			mir_connection_release(conn);
		}

		public static void Shutdown()
		{
			// TODO
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
