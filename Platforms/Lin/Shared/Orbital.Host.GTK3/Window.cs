using System.Text;
using Orbital.Numerics;
using System.Runtime.InteropServices;

namespace Orbital.Host.GTK3
{
	public unsafe sealed class Window : WindowBase
	{
		internal static List<Window> _windows = new List<Window>();
		public static IReadOnlyList<Window> windows => _windows;

		public IntPtr handle { get; private set; }
		private bool isClosed;

		public Window(Size2 size, WindowType type, WindowStartupPosition startupPosition, bool borderlessIsSplash)
		{
			Init(size.width, size.height, type, startupPosition, borderlessIsSplash);
		}

		public Window(int width, int height, WindowType type, WindowStartupPosition startupPosition, bool borderlessIsSplash)
		{
			Init(width, height, type, startupPosition, borderlessIsSplash);
		}

		private void Init(int width, int height, WindowType type, WindowStartupPosition startupPosition, bool borderlessIsSplash)
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
				var callbackHandler = GTK3.g_signal_connect(Application.app, activatePtr, GTK3.G_CALLBACK(activateFunc), &data);
				GTK3.g_application_activate(Application.app);// fire events now
				GTK3.g_signal_handler_disconnect(Application.app, callbackHandler);// disable callback
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
			data->handle = GTK3.gtk_application_window_new(app);
			GTK3.gtk_window_set_default_size(data->handle, data->width, data->height);
			
			// type
			switch (data->type)
			{
				case WindowType.Standard:
					GTK3.gtk_window_set_type_hint(data->handle, GTK3.GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_NORMAL);
					GTK3.gtk_window_set_resizable(data->handle, 1);
					break;
				
				case WindowType.Tool:
					GTK3.gtk_window_set_type_hint(data->handle, GTK3.GdkWindowTypeHint.GDK_WINDOW_TYPE_HINT_DIALOG);
					GTK3.gtk_window_set_resizable(data->handle, 0);
					break;
				
				case WindowType.Borderless:
					GTK3.gtk_window_set_decorated(data->handle, 0);
					break;
				
				case WindowType.Fullscreen:
					GTK3.gtk_window_set_decorated(data->handle, 0);
					GTK3.gtk_window_fullscreen(data->handle);
					break;
			}
			
			// center window
			if (data->startupPosition == WindowStartupPosition.CenterScreen)
			{
				GTK3.gtk_window_set_position(data->handle, GTK3.GtkWindowPosition.GTK_WIN_POS_CENTER);
			}
			
			// watch for window close
			fixed (byte* signalPtr = Encoding.ASCII.GetBytes("delete-event\0"))
			{
				WasClosedMethod wasClosedFuncPtr = WasClosed;
				GTK3.g_signal_connect(data->handle, signalPtr, GTK3.G_CALLBACK(wasClosedFuncPtr), (void*)data->handle);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void WasClosedMethod(IntPtr widget, GTK3.GdkEvent* e, void* dataPtr);
		private static void WasClosed(IntPtr widget, GTK3.GdkEvent* e, void* dataPtr)
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
				GTK3.gtk_window_set_title(handle, titlePtr);
			}
		}

		public override void Show()
		{
			GTK3.gtk_widget_show_all(handle);
		}

		public override void Close()
		{
			isClosed = true;
			_windows.Remove(this);
			if (handle != IntPtr.Zero)
			{
				GTK3.gtk_window_close(handle);
				GTK3.gtk_widget_destroy(handle);
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
			GTK3.gtk_window_get_size(handle, &width, &height);
			return new Size2(width, height);
		}
	}
}
