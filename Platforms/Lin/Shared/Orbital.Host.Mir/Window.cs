﻿using System.Text;
using System.Runtime.InteropServices;
using Orbital.Numerics;

using MirWindowSpec = System.IntPtr;
using MirWindow = System.IntPtr;
using MirBufferStream = System.IntPtr;
using MirEvent = System.IntPtr;

namespace Orbital.Host.Mir
{
	public unsafe sealed class Window : WindowBase
	{
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public MirWindow handle { get; private set; }
		internal MirBufferStream bufferStream;

		internal struct CallbackData
		{
			public MirWindow handle;
			public bool repaint;
			public bool resized;
			public bool isClosed;
		}
		internal CallbackData* callbackData;

		public Window(string title, int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			MirWindowSpec spec = MirClient.mir_create_normal_window_spec(Application.connection, Application.primaryDisplay.display.width, Application.primaryDisplay.display.height);
			try
			{
				// set window pixel format to match display
				MirClient.mir_window_spec_set_pixel_format(spec, Application.primaryDisplayPixelFormat);

				// set window title
				byte[] windowName = Encoding.UTF8.GetBytes(title + "\0");
				fixed (byte* windowNamePtr = windowName) MirClient.mir_window_spec_set_name(spec, windowNamePtr);

				// set window buffer usage to allow CPU updates
				MirClient.mir_window_spec_set_buffer_usage(spec, MirClient.MirBufferUsage.mir_buffer_usage_software);

				// create window
				MirWindow window = MirClient.mir_create_window_sync(spec);
				if (window == MirWindow.Zero) throw new Exception("Failed to create Window");

				// get buffer and set swap
				MirBufferStream bs = MirClient.mir_window_get_buffer_stream(window);
				MirClient.mir_buffer_stream_set_swapinterval(bs, 0);// TODO: should swap be set to 1 instead of 0 ??

				// listen to events
				mirWindowEventCallback = new MirWindowEventCallbackMethod(MirWindowEventCallback);// we keep this ref in memory like this so GC doesn't delete it
				var mirWindowEventCallbackPtr = Marshal.GetFunctionPointerForDelegate(mirWindowEventCallback);
				callbackData = (CallbackData*)Marshal.AllocHGlobal(Marshal.SizeOf<CallbackData>());
				*callbackData = new CallbackData();// clear memory
				callbackData->handle = handle;
				callbackData->repaint = true;// make sure window is repainted after create
				MirClient.mir_window_set_event_handler(window, mirWindowEventCallbackPtr, callbackData);

				// track window
				lock (_windows) _windows.Add(this);
			}
			finally
			{
				MirClient.mir_window_spec_release(spec);
			}
		}

		private delegate void MirWindowEventCallbackMethod(MirWindow window, MirEvent e, void* context);
		private MirWindowEventCallbackMethod mirWindowEventCallback;
		private static void MirWindowEventCallback(MirWindow mirWindow, MirEvent e, void* context)
		{
			var callbackData = (CallbackData*)context;

			// find window instance
			Window window = null;
			lock (_windows)
			{
				foreach (var w in _windows)
				{
					if (w.handle == mirWindow)
					{
						window = w;
						break;
					}
				}
				if (window == null) return;
			}

			// process event
			lock (window)
			{
				MirClient.MirEventType eventType = MirClient.mir_event_get_type(e);
				switch (eventType)
				{
					case MirClient.MirEventType.mir_event_type_resize:
						callbackData->resized = true;
						callbackData->repaint = true;
						break;

					case MirClient.MirEventType.mir_event_type_close_window:
						callbackData->isClosed = true;
						break;
				}
			}
		}

		public override void Dispose()
		{
			Close();
		}

		public override IntPtr GetHandle()
		{
			return handle;
		}

		public override object GetManagedHandle()
		{
			return this;
		}

		public override void SetTitle(string title)
		{
			// TODO
		}

		public override void Show()
		{
			// TODO
		}

		public override void Close()
		{
			lock (_windows) _windows.Remove(this);

			if (handle != MirWindow.Zero)
			{
				MirClient.mir_window_set_event_handler(handle, nint.Zero, null);
				MirClient.mir_window_release_sync(handle);
				handle = MirWindow.Zero;
			}

			if (callbackData != null)
			{
				Marshal.FreeHGlobal((IntPtr)callbackData);
				callbackData = null;
			}
		}

		public override bool IsClosed()
		{
			return callbackData->isClosed;
		}

		public override Size2 GetSize()
		{
			MirClient.MirGraphicsRegion backbuffer;
			if (MirClient.mir_buffer_stream_get_graphics_region(bufferStream, &backbuffer) == 0) throw new Exception("Get graphics region failed");
			return new Size2(backbuffer.width, backbuffer.height);
		}
	}
}
