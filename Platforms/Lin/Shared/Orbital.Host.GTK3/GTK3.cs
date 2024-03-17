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
		
		public enum GConnectFlags
		{
			G_CONNECT_AFTER	= 1 << 0,
			G_CONNECT_SWAPPED = 1 << 1
		}
		
		public enum GdkEventType
		{
			GDK_NOTHING		= -1,
			GDK_DELETE		= 0,
			GDK_DESTROY		= 1,
			GDK_EXPOSE		= 2,
			GDK_MOTION_NOTIFY	= 3,
			GDK_BUTTON_PRESS	= 4,
			GDK_2BUTTON_PRESS	= 5,
			GDK_DOUBLE_BUTTON_PRESS = GDK_2BUTTON_PRESS,
			GDK_3BUTTON_PRESS	= 6,
			GDK_TRIPLE_BUTTON_PRESS = GDK_3BUTTON_PRESS,
			GDK_BUTTON_RELEASE	= 7,
			GDK_KEY_PRESS		= 8,
			GDK_KEY_RELEASE	= 9,
			GDK_ENTER_NOTIFY	= 10,
			GDK_LEAVE_NOTIFY	= 11,
			GDK_FOCUS_CHANGE	= 12,
			GDK_CONFIGURE		= 13,
			GDK_MAP		= 14,
			GDK_UNMAP		= 15,
			GDK_PROPERTY_NOTIFY	= 16,
			GDK_SELECTION_CLEAR	= 17,
			GDK_SELECTION_REQUEST = 18,
			GDK_SELECTION_NOTIFY	= 19,
			GDK_PROXIMITY_IN	= 20,
			GDK_PROXIMITY_OUT	= 21,
			GDK_DRAG_ENTER        = 22,
			GDK_DRAG_LEAVE        = 23,
			GDK_DRAG_MOTION       = 24,
			GDK_DRAG_STATUS       = 25,
			GDK_DROP_START        = 26,
			GDK_DROP_FINISHED     = 27,
			GDK_CLIENT_EVENT	= 28,
			GDK_VISIBILITY_NOTIFY = 29,
			GDK_SCROLL            = 31,
			GDK_WINDOW_STATE      = 32,
			GDK_SETTING           = 33,
			GDK_OWNER_CHANGE      = 34,
			GDK_GRAB_BROKEN       = 35,
			GDK_DAMAGE            = 36,
			GDK_TOUCH_BEGIN       = 37,
			GDK_TOUCH_UPDATE      = 38,
			GDK_TOUCH_END         = 39,
			GDK_TOUCH_CANCEL      = 40,
			GDK_TOUCHPAD_SWIPE    = 41,
			GDK_TOUCHPAD_PINCH    = 42,
			GDK_PAD_BUTTON_PRESS  = 43,
			GDK_PAD_BUTTON_RELEASE = 44,
			GDK_PAD_RING          = 45,
			GDK_PAD_STRIP         = 46,
			GDK_PAD_GROUP_MODE    = 47,
			GDK_EVENT_LAST        /* helper variable for decls */
		}
		
		public enum GdkScrollDirection
		{
			GDK_SCROLL_UP,
			GDK_SCROLL_DOWN,
			GDK_SCROLL_LEFT,
			GDK_SCROLL_RIGHT,
			GDK_SCROLL_SMOOTH
		}
		
		public enum GdkVisibilityState
		{
			GDK_VISIBILITY_UNOBSCURED,
			GDK_VISIBILITY_PARTIAL,
			GDK_VISIBILITY_FULLY_OBSCURED
		}
		
		public enum GdkCrossingMode
		{
			GDK_CROSSING_NORMAL,
			GDK_CROSSING_GRAB,
			GDK_CROSSING_UNGRAB,
			GDK_CROSSING_GTK_GRAB,
			GDK_CROSSING_GTK_UNGRAB,
			GDK_CROSSING_STATE_CHANGED,
			GDK_CROSSING_TOUCH_BEGIN,
			GDK_CROSSING_TOUCH_END,
			GDK_CROSSING_DEVICE_SWITCH
		}
		
		public enum GdkNotifyType
		{
			GDK_NOTIFY_ANCESTOR		= 0,
			GDK_NOTIFY_VIRTUAL		= 1,
			GDK_NOTIFY_INFERIOR		= 2,
			GDK_NOTIFY_NONLINEAR		= 3,
			GDK_NOTIFY_NONLINEAR_VIRTUAL	= 4,
			GDK_NOTIFY_UNKNOWN		= 5
		}
		
		public enum GdkOwnerChange
		{
			GDK_OWNER_CHANGE_NEW_OWNER,
			GDK_OWNER_CHANGE_DESTROY,
			GDK_OWNER_CHANGE_CLOSE
		}
		
		public enum GdkWindowState
		{
			GDK_WINDOW_STATE_WITHDRAWN        = 1 << 0,
			GDK_WINDOW_STATE_ICONIFIED        = 1 << 1,
			GDK_WINDOW_STATE_MAXIMIZED        = 1 << 2,
			GDK_WINDOW_STATE_STICKY           = 1 << 3,
			GDK_WINDOW_STATE_FULLSCREEN       = 1 << 4,
			GDK_WINDOW_STATE_ABOVE            = 1 << 5,
			GDK_WINDOW_STATE_BELOW            = 1 << 6,
			GDK_WINDOW_STATE_FOCUSED          = 1 << 7,
			GDK_WINDOW_STATE_TILED            = 1 << 8,
			GDK_WINDOW_STATE_TOP_TILED        = 1 << 9,
			GDK_WINDOW_STATE_TOP_RESIZABLE    = 1 << 10,
			GDK_WINDOW_STATE_RIGHT_TILED      = 1 << 11,
			GDK_WINDOW_STATE_RIGHT_RESIZABLE  = 1 << 12,
			GDK_WINDOW_STATE_BOTTOM_TILED     = 1 << 13,
			GDK_WINDOW_STATE_BOTTOM_RESIZABLE = 1 << 14,
			GDK_WINDOW_STATE_LEFT_TILED       = 1 << 15,
			GDK_WINDOW_STATE_LEFT_RESIZABLE   = 1 << 16
		}
		
		public enum GdkSettingAction
		{
			GDK_SETTING_ACTION_NEW,
			GDK_SETTING_ACTION_CHANGED,
			GDK_SETTING_ACTION_DELETED
		}
		
		public enum GtkWindowPosition
		{
			GTK_WIN_POS_NONE,
			GTK_WIN_POS_CENTER,
			GTK_WIN_POS_MOUSE,
			GTK_WIN_POS_CENTER_ALWAYS,
			GTK_WIN_POS_CENTER_ON_PARENT
		}
		
		public enum GdkWindowTypeHint
		{
			GDK_WINDOW_TYPE_HINT_NORMAL,
			GDK_WINDOW_TYPE_HINT_DIALOG,
			GDK_WINDOW_TYPE_HINT_MENU,		/* Torn off menu */
			GDK_WINDOW_TYPE_HINT_TOOLBAR,
			GDK_WINDOW_TYPE_HINT_SPLASHSCREEN,
			GDK_WINDOW_TYPE_HINT_UTILITY,
			GDK_WINDOW_TYPE_HINT_DOCK,
			GDK_WINDOW_TYPE_HINT_DESKTOP,
			GDK_WINDOW_TYPE_HINT_DROPDOWN_MENU,	/* A drop down menu (from a menubar) */
			GDK_WINDOW_TYPE_HINT_POPUP_MENU,	/* A popup menu (from right-click) */
			GDK_WINDOW_TYPE_HINT_TOOLTIP,
			GDK_WINDOW_TYPE_HINT_NOTIFICATION,
			GDK_WINDOW_TYPE_HINT_COMBO,
			GDK_WINDOW_TYPE_HINT_DND
		}
		
		public enum GdkGravity
		{
			GDK_GRAVITY_NORTH_WEST = 1,
			GDK_GRAVITY_NORTH,
			GDK_GRAVITY_NORTH_EAST,
			GDK_GRAVITY_WEST,
			GDK_GRAVITY_CENTER,
			GDK_GRAVITY_EAST,
			GDK_GRAVITY_SOUTH_WEST,
			GDK_GRAVITY_SOUTH,
			GDK_GRAVITY_SOUTH_EAST,
			GDK_GRAVITY_STATIC
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventAny
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkRectangle
		{
			public int x, y;
			public int width, height;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventExpose
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkRectangle area;
			public cairo_region_t region;
			public gint count; /* If non-zero, how many more events follow. */
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventVisibility
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkVisibilityState state;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventMotion
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public gdouble *axes;
			public guint state;
			public gint16 is_hint;
			public GdkDevice device;
			public gdouble x_root, y_root;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventButton
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public gdouble *axes;
			public guint state;
			public guint button;
			public GdkDevice device;
			public gdouble x_root, y_root;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventTouch
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public gdouble *axes;
			public guint state;
			public GdkEventSequence sequence;
			public gboolean emulating_pointer;
			public GdkDevice device;
			public gdouble x_root, y_root;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventScroll
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public guint state;
			public GdkScrollDirection direction;
			public GdkDevice device;
			public gdouble x_root, y_root;
			public gdouble delta_x;
			public gdouble delta_y;
			public bool is_stop;//public guint is_stop : 1;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventKey
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public guint state;
			public guint keyval;
			public gint length;
			public gchar *_string;
			public guint16 hardware_keycode;
			public guint8 group;
			public bool is_modifier;//public guint is_modifier : 1;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventCrossing
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkWindow subwindow;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public gdouble x_root;
			public gdouble y_root;
			public GdkCrossingMode mode;
			public GdkNotifyType detail;
			public gboolean focus;
			public guint state;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventFocus
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public gint16 _in;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventConfigure
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public gint x, y;
			public gint width;
			public gint height;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventProperty
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkAtom atom;
			public guint32 time;
			public guint state;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventSelection
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkAtom selection;
			public GdkAtom target;
			public GdkAtom property;
			public guint32 time;
			public GdkWindow requestor;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventOwnerChange
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkWindow owner;
			public GdkOwnerChange reason;
			public GdkAtom selection;
			public guint32 time;
			public guint32 selection_time;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventProximity
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public GdkDevice device;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventDND
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkDragContext context;
			public guint32 time;
			public gshort x_root, y_root;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventWindowState
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkWindowState changed_mask;
			public GdkWindowState new_window_state;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventSetting
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public GdkSettingAction action;
			public byte *name;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventGrabBroken
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public gboolean keyboard;
			public gboolean _implicit;
			public GdkWindow grab_window;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventTouchpadSwipe
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public gint8 phase;
			public gint8 n_fingers;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public gdouble dx;
			public gdouble dy;
			public gdouble x_root, y_root;
			public guint state;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventTouchpadPinch
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public gint8 phase;
			public gint8 n_fingers;
			public guint32 time;
			public gdouble x;
			public gdouble y;
			public gdouble dx;
			public gdouble dy;
			public gdouble angle_delta;
			public gdouble scale;
			public gdouble x_root, y_root;
			public guint state;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventPadButton
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public guint group;
			public guint button;
			public guint mode;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventPadAxis
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public guint group;
			public guint index;
			public guint mode;
			public gdouble value;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct GdkEventPadGroupMode
		{
			public GdkEventType type;
			public GdkWindow window;
			public gint8 send_event;
			public guint32 time;
			public guint group;
			public guint mode;
		}
		
		[StructLayout(LayoutKind.Explicit)]
		public struct GdkEvent
		{
			[FieldOffset(0)] public GdkEventType		    type;
			[FieldOffset(0)] public GdkEventAny		    any;
			[FieldOffset(0)] public GdkEventExpose	    expose;
			[FieldOffset(0)] public GdkEventVisibility	    visibility;
			[FieldOffset(0)] public GdkEventMotion	    motion;
			[FieldOffset(0)] public GdkEventButton	    button;
			[FieldOffset(0)] public GdkEventTouch             touch;
			[FieldOffset(0)] public GdkEventScroll            scroll;
			[FieldOffset(0)] public GdkEventKey		    key;
			[FieldOffset(0)] public GdkEventCrossing	    crossing;
			[FieldOffset(0)] public GdkEventFocus		    focus_change;
			[FieldOffset(0)] public GdkEventConfigure	    configure;
			[FieldOffset(0)] public GdkEventProperty	    property;
			[FieldOffset(0)] public GdkEventSelection	    selection;
			[FieldOffset(0)] public GdkEventOwnerChange  	    owner_change;
			[FieldOffset(0)] public GdkEventProximity	    proximity;
			[FieldOffset(0)] public GdkEventDND               dnd;
			[FieldOffset(0)] public GdkEventWindowState       window_state;
			[FieldOffset(0)] public GdkEventSetting           setting;
			[FieldOffset(0)] public GdkEventGrabBroken        grab_broken;
			[FieldOffset(0)] public GdkEventTouchpadSwipe     touchpad_swipe;
			[FieldOffset(0)] public GdkEventTouchpadPinch     touchpad_pinch;
			[FieldOffset(0)] public GdkEventPadButton         pad_button;
			[FieldOffset(0)] public GdkEventPadAxis           pad_axis;
			[FieldOffset(0)] public GdkEventPadGroupMode      pad_group_mode;
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
		public static extern void gtk_widget_show_all(IntPtr widget);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_close(IntPtr window);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_position(IntPtr window, GtkWindowPosition position);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_type_hint(IntPtr window, GdkWindowTypeHint hint);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_gravity(IntPtr window, GdkGravity gravity);

		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_fullscreen(IntPtr window);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_resizable(IntPtr window, gboolean resizable);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_get_size(IntPtr window, gint* width, gint* height);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_window_set_decorated(IntPtr window, gboolean setting);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gtk_widget_destroy(IntPtr widget);
		
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
		public static extern int gdk_display_get_n_monitors(IntPtr display);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_display_get_monitor(IntPtr display, int monitor_num);

		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_display_get_default();
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern IntPtr gdk_display_get_primary_monitor(IntPtr display);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gdk_monitor_get_workarea(IntPtr monitor, GdkRectangle* workarea);
		
		[DllImport(lib, ExactSpelling = true)]
		public static extern void gdk_monitor_get_geometry(IntPtr monitor, GdkRectangle* workarea);
	}
}
