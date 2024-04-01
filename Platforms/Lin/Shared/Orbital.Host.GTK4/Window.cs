using System.Text;
using Orbital.Numerics;
using System.Runtime.InteropServices;

namespace Orbital.Host.GTK4
{
	public unsafe sealed class Window : WindowBase
	{
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public IntPtr handle { get; private set; }
		private bool isClosed;

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(size.width, size.height, type, startupPosition);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			Init(width, height, type, startupPosition);
		}

		private void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition)
		{
			var data = new CallbackData()
			{
				width = width,
				height = height,
				type = type,
				startupPosition = startupPosition
			};
			
			// create window
			fixed (byte* activatePtr = Encoding.ASCII.GetBytes("activate\0"))
			{
				ActivateMethod activateFunc = Activate;
				var callbackHandler = GTK4.g_signal_connect(Application.app, activatePtr, GTK4.G_CALLBACK(activateFunc), &data);
				GTK4.g_application_activate(Application.app);// fire events now
				GTK4.g_signal_handler_disconnect(Application.app, callbackHandler);// disable callback
				handle = data.handle;
			}

			// track window
			_windows.Add(this);
		}
		
		struct CallbackData
		{
			public IntPtr handle;
			public int width, height;
			public WindowType type;
			public WindowStartupPosition startupPosition;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void ActivateMethod(IntPtr app, void* dataPtr);
		private static void Activate(IntPtr app, void* dataPtr)
		{
			if (dataPtr == null) return;
			var data = (CallbackData*)dataPtr;
			
			// create window
			data->handle = GTK4.gtk_application_window_new(app);
			GTK4.gtk_window_set_default_size(data->handle, data->width, data->height);
			
			// type
			switch (data->type)
			{
				case WindowType.Standard:
					GTK4.gtk_window_set_resizable(data->handle, 1);
					break;
				
				case WindowType.Tool:// NOTE: GTK4 has no way to disable minimize button
					GTK4.gtk_window_set_resizable(data->handle, 0);
					break;
				
				case WindowType.Borderless:
					GTK4.gtk_window_set_decorated(data->handle, 0);
					GTK4.gtk_window_set_resizable(data->handle, 0);
					break;
				
				case WindowType.Fullscreen:
					GTK4.gtk_window_set_decorated(data->handle, 0);
					GTK4.gtk_window_set_resizable(data->handle, 0);
					GTK4.gtk_window_fullscreen(data->handle);
					break;
			}
			
			// center window
			if (data->startupPosition == WindowStartupPosition.CenterScreen)
			{
				// NOTE: this is not possible in GTK4
			}
			
			// watch for window close
			fixed (byte* signalPtr = Encoding.ASCII.GetBytes("close-request\0"))
			{
				WasClosedMethod wasClosedFuncPtr = WasClosed;
				GTK4.g_signal_connect(data->handle, signalPtr, GTK4.G_CALLBACK(wasClosedFuncPtr), (void*)data->handle);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void WasClosedMethod(IntPtr widget, void* dataPtr);
		private static void WasClosed(IntPtr widget, void* dataPtr)
		{
			var handle = (IntPtr)dataPtr;
			foreach (var window in _windows)
			{
				if (window.handle == handle)
				{
					window.Close();
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
			fixed (byte* titlePtr = Encoding.ASCII.GetBytes(title + "\0"))
			{
				GTK4.gtk_window_set_title(handle, titlePtr);
			}
		}

		public override void Show()
		{
			GTK4.gtk_window_present(handle);
		}

		public override void Close()
		{
			isClosed = true;
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				GTK4.gtk_window_close(handle);
				GTK4.gtk_window_destroy(handle);
				handle = IntPtr.Zero;
			}
		}

		public override bool IsClosed()
		{
			return isClosed;
		}

		public override Size2 GetSize()
		{
			int width, height;
			GTK4.gtk_window_get_default_size(handle, &width, &height);
			return new Size2(width, height);
		}
	}
}
