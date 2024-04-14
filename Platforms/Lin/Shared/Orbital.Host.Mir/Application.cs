using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using Orbital.OS.Lin;

using MirConnection = System.IntPtr;

namespace Orbital.Host.Mir
{
	public unsafe static class Application
	{
		public static MirConnection connection { get; private set; }
		public static DisplayEx primaryDisplay { get; private set; }
		public static MirClient.MirPixelFormat primaryDisplayPixelFormat { get; private set; }
		private static bool primaryDisplayPixelFormat_isABGR;
		private static bool exit;

		public static void Init(string appID)
		{
			LibraryResolver.Init(Assembly.GetExecutingAssembly());

			// connect to display server
			byte[] appName = Encoding.UTF8.GetBytes(appID + "\0");
			fixed (byte* appNamePtr = appName) connection = MirClient.mir_connect_sync(null, appNamePtr);
			if (MirClient.mir_connection_is_valid(connection) == 0)
			{
				byte* ePtr = MirClient.mir_connection_get_error_message(connection);
				string e = Marshal.PtrToStringAnsi((IntPtr)ePtr);
				throw new Exception(string.Format("Could not connect to a display server: '{0}'", e));
			}

			// validate RGBA8 format exists
			bool hasFormat_abgr = false;
			bool hasFormat_xbgr = false;
			bool hasFormat_argb = false;
			bool hasFormat_xrgb = false;
			primaryDisplay = Displays.GetPrimaryDisplayEx();
			for (int i = 0; i != primaryDisplay.formats.Length; ++i)
			{
				if (Displays.GetPixelFormatByteCount(primaryDisplay.formats[i]) == 4)
				{
					switch (primaryDisplay.formats[i])
					{
						case MirClient.MirPixelFormat.mir_pixel_format_abgr_8888: hasFormat_abgr = true; break;
						case MirClient.MirPixelFormat.mir_pixel_format_xbgr_8888: hasFormat_xbgr = true; break;
						case MirClient.MirPixelFormat.mir_pixel_format_argb_8888: hasFormat_argb = true; break;
						case MirClient.MirPixelFormat.mir_pixel_format_xrgb_8888: hasFormat_xrgb = true; break;
					}
				}
			}

			primaryDisplayPixelFormat = MirClient.MirPixelFormat.mir_pixel_format_invalid;
			primaryDisplayPixelFormat_isABGR = true;
			if (hasFormat_abgr)
			{
				primaryDisplayPixelFormat = MirClient.MirPixelFormat.mir_pixel_format_abgr_8888;
				primaryDisplayPixelFormat_isABGR = true;
			}
			else if (hasFormat_xbgr)
			{
				primaryDisplayPixelFormat = MirClient.MirPixelFormat.mir_pixel_format_xbgr_8888;
				primaryDisplayPixelFormat_isABGR = true;
			}
			else if (hasFormat_argb)
			{
				primaryDisplayPixelFormat = MirClient.MirPixelFormat.mir_pixel_format_argb_8888;
				primaryDisplayPixelFormat_isABGR = false;
			}
			else if (hasFormat_xrgb)
			{
				primaryDisplayPixelFormat = MirClient.MirPixelFormat.mir_pixel_format_xrgb_8888;
				primaryDisplayPixelFormat_isABGR = false;
			}
			else
			{
				throw new Exception("No valid 32-bit format found");
			}
		}

		public static void Shutdown()
		{
			// close all windows
			for (int i = Window._windows.Count - 1; i >= 0; --i)
			{
				Window._windows[i].Close();
			}

			// release display server connection
			if (connection != MirConnection.Zero)
			{
				MirClient.mir_connection_release(connection);
				connection = MirConnection.Zero;
			}
		}

		private static void UpdateWindow(Window window)
		{
			if (window.callbackData->resized)
			{
				window.callbackData->resized = false;

				// get buffer
				MirClient.MirGraphicsRegion backbuffer;
				MirClient.mir_buffer_stream_get_graphics_region(window.bufferStream, &backbuffer);

				// clear buffer
				byte* data = backbuffer.vaddr;
				int size = backbuffer.width * backbuffer.height * 4;
				for (int i = 0; i < size; i += 4)
				{
					data[i + 0] = 255;// R or B
					data[i + 1] = 255;// G
					data[i + 2] = 255;// R or B
					data[i + 3] = 255;// A
				}

				// swap buffer
				MirClient.mir_buffer_stream_swap_buffers_sync(window.bufferStream);
			}
		}

		public static void Run()
		{
			while (!exit && Window._windows.Count != 0)
			{
				foreach (var window in Window.windows)
				{
					UpdateWindow(window);
				}
			}
		}

		public static void Run(Window window)
		{
			while (!exit && !window.IsClosed())
			{
				foreach (var w in Window._windows)
				{
					UpdateWindow(w);
				}
			}
		}

		public static void RunEvents()
		{
			foreach (var window in Window._windows)
			{
				UpdateWindow(window);
			}
		}

		public static void Exit()
		{
			exit = true;
		}
	}
}
