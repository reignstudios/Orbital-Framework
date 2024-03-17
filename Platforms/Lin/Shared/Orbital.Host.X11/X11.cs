using System;
using System.Runtime.InteropServices;
using System.Security;

using XID = System.IntPtr;
using WindowX = System.IntPtr;
using Drawable = System.IntPtr;
using Colormap = System.IntPtr;
using Pixmap = System.IntPtr;
using Cursor = System.IntPtr;
using VisualID = System.IntPtr;
using Bool = System.Int32;
using Long = System.IntPtr;
using ULong = System.UIntPtr;
using Time = System.UIntPtr;
using Atom = System.IntPtr;
using Status = System.Int32;

namespace Orbital.Host.X11
{
	public unsafe static class X11
	{
		public const string DLL = "libX11.so";
		
		public const int KeyPress = 2;
		public const int KeyRelease = 3;
		public const int ButtonPress = 4;
		public const int ButtonRelease = 5;
		public const int MotionNotify = 6;
		public const int EnterNotify = 7;
		public const int LeaveNotify = 8;
		public const int FocusIn = 9;
		public const int FocusOut = 10;
		public const int KeymapNotify = 11;
		public const int Expose = 12;
		public const int GraphicsExpose = 13;
		public const int NoExpose = 14;
		public const int VisibilityNotify = 15;
		public const int CreateNotify = 16;
		public const int DestroyNotify = 17;
		public const int UnmapNotify = 18;
		public const int MapNotify = 19;
		public const int MapRequest = 20;
		public const int ReparentNotify = 21;
		public const int ConfigureNotify = 22;
		public const int ConfigureRequest = 23;
		public const int GravityNotify = 24;
		public const int ResizeRequest = 25;
		public const int CirculateNotify = 26;
		public const int CirculateRequest = 27;
		public const int PropertyNotify = 28;
		public const int SelectionClear = 29;
		public const int SelectionRequest = 30;
		public const int SelectionNotify = 31;
		public const int ColormapNotify = 32;
		public const int ClientMessage = 33;
		public const int MappingNotify = 34;
		public const int GenericEvent = 35;
		public const int LASTEvent = 36;
		
		public const int NoEventMask = 0;
		public const int KeyPressMask = (1<<0);
		public const int KeyReleaseMask = (1<<1);
		public const int ButtonPressMask = (1<<2);
		public const int ButtonReleaseMask = (1<<3);
		public const int EnterWindowMask = (1<<4);
		public const int LeaveWindowMask = (1<<5);
		public const int PointerMotionMask = (1<<6);
		public const int PointerMotionHintMask = (1<<7);
		public const int Button1MotionMask = (1<<8);
		public const int Button2MotionMask = (1<<9);
		public const int Button3MotionMask = (1<<10);
		public const int Button4MotionMask = (1<<11);
		public const int Button5MotionMask = (1<<12);
		public const int ButtonMotionMask = (1<<13);
		public const int KeymapStateMask = (1<<14);
		public const int ExposureMask = (1<<15);
		public const int VisibilityChangeMask = (1<<16);
		public const int StructureNotifyMask = (1<<17);
		public const int ResizeRedirectMask = (1<<18);
		public const int SubstructureNotifyMask = (1<<19);
		public const int SubstructureRedirectMask = (1<<20);
		public const int FocusChangeMask = (1<<21);
		public const int PropertyChangeMask = (1<<22);
		public const int ColormapChangeMask = (1<<23);
		public const int OwnerGrabButtonMask = (1<<24);
		
		/*public enum XRequest : byte
	    {
	        X_CreateWindow = 1,
	        X_ChangeWindowAttributes = 2,
	        X_GetWindowAttributes = 3,
	        X_DestroyWindow = 4,
	        X_DestroySubwindows = 5,
	        X_ChangeSaveSet = 6,
	        X_ReparentWindow = 7,
	        X_MapWindow = 8,
	        X_MapSubwindows = 9,
	        X_UnmapWindow = 10,
	        X_UnmapSubwindows = 11,
	        X_ConfigureWindow = 12,
	        X_CirculateWindow = 13,
	        X_GetGeometry = 14,
	        X_QueryTree = 15,
	        X_InternAtom = 16,
	        X_GetAtomName = 17,
	        X_ChangeProperty = 18,
	        X_DeleteProperty = 19,
	        X_GetProperty = 20,
	        X_ListProperties = 21,
	        X_SetSelectionOwner = 22,
	        X_GetSelectionOwner = 23,
	        X_ConvertSelection = 24,
	        X_SendEvent = 25,
	        X_GrabPointer = 26,
	        X_UngrabPointer = 27,
	        X_GrabButton = 28,
	        X_UngrabButton = 29,
	        X_ChangeActivePointerGrab = 30,
	        X_GrabKeyboard = 31,
	        X_UngrabKeyboard = 32,
	        X_GrabKey = 33,
	        X_UngrabKey = 34,
	        X_AllowEvents = 35,
	        X_GrabServer = 36,
	        X_UngrabServer = 37,
	        X_QueryPointer = 38,
	        X_GetMotionEvents = 39,
	        X_TranslateCoords = 40,
	        X_WarpPointer = 41,
	        X_SetInputFocus = 42,
	        X_GetInputFocus = 43,
	        X_QueryKeymap = 44,
	        X_OpenFont = 45,
	        X_CloseFont = 46,
	        X_QueryFont = 47,
	        X_QueryTextExtents = 48,
	        X_ListFonts = 49,
	        X_ListFontsWithInfo = 50,
	        X_SetFontPath = 51,
	        X_GetFontPath = 52,
	        X_CreatePixmap = 53,
	        X_FreePixmap = 54,
	        X_CreateGC = 55,
	        X_ChangeGC = 56,
	        X_CopyGC = 57,
	        X_SetDashes = 58,
	        X_SetClipRectangles = 59,
	        X_FreeGC = 60,
	        X_ClearArea = 61,
	        X_CopyArea = 62,
	        X_CopyPlane = 63,
	        X_PolyPoint = 64,
	        X_PolyLine = 65,
	        X_PolySegment = 66,
	        X_PolyRectangle = 67,
	        X_PolyArc = 68,
	        X_FillPoly = 69,
	        X_PolyFillRectangle = 70,
	        X_PolyFillArc = 71,
	        X_PutImage = 72,
	        X_GetImage = 73,
	        X_PolyText8 = 74,
	        X_PolyText16 = 75,
	        X_ImageText8 = 76,
	        X_ImageText16 = 77,
	        X_CreateColormap = 78,
	        X_FreeColormap = 79,
	        X_CopyColormapAndFree = 80,
	        X_InstallColormap = 81,
	        X_UninstallColormap = 82,
	        X_ListInstalledColormaps = 83,
	        X_AllocColor = 84,
	        X_AllocNamedColor = 85,
	        X_AllocColorCells = 86,
	        X_AllocColorPlanes = 87,
	        X_FreeColors = 88,
	        X_StoreColors = 89,
	        X_StoreNamedColor = 90,
	        X_QueryColors = 91,
	        X_LookupColor = 92,
	        X_CreateCursor = 93,
	        X_CreateGlyphCursor = 94,
	        X_FreeCursor = 95,
	        X_RecolorCursor = 96,
	        X_QueryBestSize = 97,
	        X_QueryExtension = 98,
	        X_ListExtensions = 99,
	        X_ChangeKeyboardMapping = 100,
	        X_GetKeyboardMapping = 101,
	        X_ChangeKeyboardControl = 102,
	        X_GetKeyboardControl = 103,
	        X_Bell = 104,
	        X_ChangePointerControl = 105,
	        X_GetPointerControl = 106,
	        X_SetScreenSaver = 107,
	        X_GetScreenSaver = 108,
	        X_ChangeHosts = 109,
	        X_ListHosts = 110,
	        X_SetAccessControl = 111,
	        X_SetCloseDownMode = 112,
	        X_KillClient = 113,
	        X_RotateProperties = 114,
	        X_ForceScreenSaver = 115,
	        X_SetPointerMapping = 116,
	        X_GetPointerMapping = 117,
	        X_SetModifierMapping = 118,
	        X_GetModifierMapping = 119,
	        X_NoOperation = 127
	    }
	    
		public enum NotifyMode : int
	    {
	        NotifyNormal = 0,
	        NotifyGrab = 1,
	        NotifyUngrab = 2
	    }
    
		public enum NotifyDetail : int
	    {
	        NotifyAncestor = 0,
	        NotifyVirtual = 1,
	        NotifyInferior = 2,
	        NotifyNonlinear = 3,
	        NotifyNonlinearVirtual = 4,
	        NotifyPointer = 5,
	        NotifyPointerRoot = 6,
	        NotifyDetailNone = 7
	    }*/
	    
		[StructLayout(LayoutKind.Sequential)]
	    public struct XAnyEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XKeyEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public WindowX root;
	        public WindowX subwindow;
	        public Time time;
	        public int x, y;
	        public int x_root, y_root;
	        public uint state;
	        public uint keycode;
	        public Bool same_screen;
	    }
	    
		[StructLayout(LayoutKind.Sequential)]
	    public struct XButtonEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public WindowX root;
	        public WindowX subwindow;
	        public Time time;
	        public int x, y;
	        public int x_root, y_root;
	        public uint state;
	        public uint button;
	        public Bool same_screen;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XMotionEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public WindowX root;
	        public WindowX subwindow;
	        public Time time;
	        public int x, y;
	        public int x_root, y_root;
	        public uint state;
	        public byte is_hint;
	        public Bool same_screen;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XCrossingEvent
	    {
	        public int type;
	        public ULong serial;
	        public bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public WindowX root;
	        public WindowX subwindow;
	        public Time time;
	        public int x, y;
	        public int x_root, y_root;
	        public int mode;
	        public int detail;
	        public Bool same_screen;
	        public Bool focus;
	        public uint state;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XFocusChangeEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public int mode;
	        public int detail;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XExposeEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public int x, y;
	        public int width, height;
	        public int count;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XGraphicsExposeEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public Drawable drawable;
	        public int x, y;
	        public int width, height;
	        public int count;
	        public int major_code;
	        public int minor_code;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XNoExposeEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public Drawable drawable;
	        public int major_code;
	        public int minor_code;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XVisibilityEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public int state;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XCreateWindowEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX parent;
	        public WindowX window;
	        public int x, y;
	        public int width, height;
	        public int border_width;
	        public Bool override_redirect;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XDestroyWindowEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XUnmapEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	        public Bool from_configure;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XMapEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	        public Bool override_redirect;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XMapRequestEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX parent;
	        public WindowX window;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XReparentEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	        public WindowX parent;
	        public int x, y;
	        public Bool override_redirect;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XConfigureEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	        public int x, y;
	        public int width, height;
	        public int border_width;
	        public WindowX above;
	        public Bool override_redirect;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XGravityEvent
	    {
	        public int type;
	        public ULong serial;
	        public bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	        public int x, y;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XResizeRequestEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public int width, height;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XConfigureRequestEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX parent;
	        public WindowX window;
	        public int x, y;
	        public int width, height;
	        public int border_width;
	        public WindowX above;
	        public int detail;
	        public ULong value_mask;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XCirculateEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX eventX;
	        public WindowX window;
	        public int place;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XCirculateRequestEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX parent;
	        public WindowX window;
	        public int place;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XPropertyEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public Atom atom;
	        public Time time;
	        public int state;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XSelectionClearEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public Atom selection;
	        public Time time;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XSelectionRequestEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX owner;
	        public WindowX requestor;
	        public Atom selection;
	        public Atom target;
	        public Atom property;
	        public Time time;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XSelectionEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX requestor;
	        public Atom selection;
	        public Atom target;
	        public Atom property;
	        public Time time;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XColormapEvent
	    {
	        public int type;
	        public ULong serial;
	        public bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public Colormap colormap;
	        public Bool c_new;
	        public int state;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XClientMessageEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public Atom message_type;
	        public int format;
	        public Long data1;
	        public Long data2;
	        public Long data3;
	        public Long data4;
	        public Long data5;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XMappingEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public int request;
	        public int first_keycode;
	        public int count;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XErrorEvent
	    {
	        public int type;
	        public IntPtr display;
	        public XID resourceid;
	        public ULong serial;
	        public byte error_code;
	        public byte request_code;
	        public byte minor_code;
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XKeymapEvent
	    {
	        public int type;
	        public ULong serial;
	        public Bool send_event;
	        public IntPtr display;
	        public WindowX window;
	        public fixed byte key_vector[32];
	    }
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XGenericEvent
		{
			public int type;
			public ULong serial;
			public Bool send_event;
			public IntPtr display;
			public int extension;
			public int evtype;
		}
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XGenericEventCookie
	    {
		    public int type;
		    public ULong serial;
		    public Bool send_event;
		    public IntPtr display;
		    public int extension;
		    public int evtype;
		    public uint cookie;
		    public void* data;
		}
    
		[StructLayout(LayoutKind.Explicit)]
	    public struct XEvent
	    {
			[FieldOffset(0)] public int type;
	        [FieldOffset(0)] public XAnyEvent xany;
	        [FieldOffset(0)] public XKeyEvent xkey;
	        [FieldOffset(0)] public XButtonEvent xbutton;
	        [FieldOffset(0)] public XMotionEvent xmotion;
	        [FieldOffset(0)] public XCrossingEvent xcrossing;
	        [FieldOffset(0)] public XFocusChangeEvent xfocus;
	        [FieldOffset(0)] public XExposeEvent xexpose;
	        [FieldOffset(0)] public XGraphicsExposeEvent xgraphicsexpose;
	        [FieldOffset(0)] public XNoExposeEvent xnoexpose;
	        [FieldOffset(0)] public XVisibilityEvent xvisibility;
	        [FieldOffset(0)] public XCreateWindowEvent xcreatewindow;
	        [FieldOffset(0)] public XDestroyWindowEvent xdestroywindow;
	        [FieldOffset(0)] public XUnmapEvent xunmap;
	        [FieldOffset(0)] public XMapEvent xmap;
	        [FieldOffset(0)] public XMapRequestEvent xmaprequest;
	        [FieldOffset(0)] public XReparentEvent xreparent;
	        [FieldOffset(0)] public XConfigureEvent xconfigure;
	        [FieldOffset(0)] public XGravityEvent xgravity;
	        [FieldOffset(0)] public XResizeRequestEvent xresizerequest;
	        [FieldOffset(0)] public XConfigureRequestEvent xconfigurerequest;
	        [FieldOffset(0)] public XCirculateEvent xcirculate;
	        [FieldOffset(0)] public XCirculateRequestEvent xcirculaterequest;
	        [FieldOffset(0)] public XPropertyEvent xproperty;
	        [FieldOffset(0)] public XSelectionClearEvent xselectionclear;
	        [FieldOffset(0)] public XSelectionRequestEvent xselectionrequest;
	        [FieldOffset(0)] public XSelectionEvent xselection;
	        [FieldOffset(0)] public XColormapEvent xcolormap;
	        [FieldOffset(0)] public XClientMessageEvent xclient;
	        [FieldOffset(0)] public XMappingEvent xmapping;
	        [FieldOffset(0)] public XErrorEvent xerror;
	        [FieldOffset(0)] public XKeymapEvent xkeymap;
	        [FieldOffset(0)] public XGenericEvent xgeneric;
	        [FieldOffset(0)] public XGenericEventCookie xcookie;
	        [FieldOffset(0)] public XEvent_Padding pad;
	    }

		[StructLayout(LayoutKind.Sequential)]
		public struct XEvent_Padding
		{
			public Long pad1, pad2, pad3, pad4, pad5, pad6, pad7, pad8, pad9, pad10, pad11, pad12, pad13, pad14, pad15, pad16, pad17, pad18, pad19, pad20, pad21, pad22, pad23, pad24;
		}
	    
	    /*public enum Gravity : int
	    {
	        ForgetGravity = 0,
	        NorthWestGravity = 1,
	        NorthGravity = 2,
	        NorthEastGravity = 3,
	        WestGravity = 4,
	        CenterGravity = 5,
	        EastGravity = 6,
	        SouthWestGravity = 7,
	        SouthGravity = 8,
	        SouthEastGravity = 9,
	        StaticGravity = 10
	    }*/
	    
	    [StructLayout(LayoutKind.Sequential)]
	    public struct XSetWindowAttributes
	    {
	        public Pixmap background_pixmap;
	        public ULong background_pixel;
	        public Pixmap border_pixmap;
	        public ULong border_pixel;
	        public int bit_gravity;
	        public int win_gravity;
	        public int backing_store;
	        public ULong backing_planes;
	        public ULong backing_pixel;
	        public Bool save_under;
	        public Long event_mask;
	        public Long do_not_propagate_mask;
	        public Bool override_redirect;
	        public Colormap colormap;
	        public Cursor cursor;
	    }
		
		[StructLayout(LayoutKind.Sequential)]
		public struct XWindowAttributes
		{
			 public int x, y;
			 public int width, height;
			 public int border_width;
			 public int depth;
			 public IntPtr visual;
			 public WindowX root;
			 public int c_class;
			 public int bit_gravity;
			 public int win_gravity;
			 public int backing_store;
			 public ULong backing_planes;
			 public ULong backing_pixel;
			 public Bool save_under;
			 public Colormap colormap;
			 public Bool map_installed;
			 public int map_state;
			 public Long all_event_masks;
			 public Long your_event_mask;
			 public Long do_not_propagate_mask;
			 public Bool override_redirect;
			 public IntPtr screen;
		}
		
		[StructLayout(LayoutKind.Sequential)] 
		public struct XVisualInfo
		{
			public IntPtr visual;
			public VisualID visualid;
			public int screen;
			public int depth;
			public int c_class;
			public ULong red_mask;
			public ULong green_mask;
			public ULong blue_mask;
			public int colormap_size;
			public int bits_per_rgb;
		}
		
		[Flags]
		public enum XSizeHintsFlags
		{
		    USPosition = (1 << 0),
		    USSize = (1 << 1),
		    PPosition = (1 << 2),
		    PSize = (1 << 3),
		    PMinSize = (1 << 4),
		    PMaxSize = (1 << 5),
		    PResizeInc = (1 << 6),
		    PAspect = (1 << 7),
		    PAllHints = (PPosition | PSize | PMinSize | PMaxSize | PResizeInc | PAspect),
		    PBaseSize = (1 << 8),
		    PWinGravity = (1 << 9),
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct XSizeHints
		{
		    public IntPtr flags;
			public int x, y;
			public int width, height;
			public int min_width, min_height;
			public int max_width, max_height;
			public int width_inc, height_inc;
			public int min_aspectX, min_aspectY;
			public int max_aspectX, max_aspectY;
			public int base_width, base_height;
			public int win_gravity;
		}
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XSetWMNormalHints", ExactSpelling = true)]
		public static extern void XSetWMNormalHints(IntPtr display, WindowX w, XSizeHints* hints);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XSetNormalHints", ExactSpelling = true)]
		public static extern int XSetNormalHints(IntPtr display, WindowX w, XSizeHints* hints);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XDisplayWidth", ExactSpelling = true)]
		public static extern int XDisplayWidth(IntPtr display, int screen_number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XDisplayHeight", ExactSpelling = true)]
		public static extern int XDisplayHeight(IntPtr display, int screen_number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XGetWindowAttributes", ExactSpelling = true)]
		public static extern int XGetWindowAttributes(IntPtr display, WindowX w, XWindowAttributes* window_attributes_return);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XOpenDisplay", ExactSpelling = true)]
		public static extern IntPtr XOpenDisplay(byte* display_name);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XDefaultScreen", ExactSpelling = true)]
		public static extern int XDefaultScreen(IntPtr dpy);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XDefaultScreenOfDisplay", ExactSpelling = true)]
		public static extern IntPtr XDefaultScreenOfDisplay(IntPtr dpy);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XScreenOfDisplay", ExactSpelling = true)]
		public static extern IntPtr XScreenOfDisplay(IntPtr display, int screen_number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XScreenCount", ExactSpelling = true)]
		public static extern int XScreenCount(IntPtr display);
		
		[SuppressUnmanagedCodeSecurity]	
		[DllImport(DLL, EntryPoint = "XCloseDisplay", ExactSpelling = true)]
		public static extern int XCloseDisplay(IntPtr display);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XDestroyWindow", ExactSpelling = true)]
		public static extern int XDestroyWindow(IntPtr display, WindowX w);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XWidthOfScreen", ExactSpelling = true)]
		public static extern int XWidthOfScreen(IntPtr screen);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XHeightOfScreen", ExactSpelling = true)]
		public static extern int XHeightOfScreen(IntPtr screen);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XCreateSimpleWindow", ExactSpelling = true)]
		public static extern IntPtr XCreateSimpleWindow(IntPtr display, WindowX parent, int x, int y, uint width, uint height, uint border_width, ULong border, ULong background);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XCreateWindow", ExactSpelling = true)]
		public static extern WindowX XCreateWindow(IntPtr display, WindowX parent, int x, int y, uint width, uint height, uint border_width, int depth, uint _class, IntPtr visual, ULong valuemask, XSetWindowAttributes* attributes);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XDefaultRootWindow", ExactSpelling = true)]
		public static extern WindowX XDefaultRootWindow(IntPtr display);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XRootWindow", ExactSpelling = true)]
		public static extern WindowX XRootWindow(IntPtr display, int screen_number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XBlackPixel", ExactSpelling = true)]
		public static extern ULong XBlackPixel(IntPtr display, int screen_number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XWhitePixel", ExactSpelling = true)]
		public static extern ULong XWhitePixel(IntPtr display, int screen_number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XSelectInput", ExactSpelling = true)]
		public static extern int XSelectInput(IntPtr display, WindowX w, Long event_mask);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XMapWindow", ExactSpelling = true)]
		public static extern int XMapWindow(IntPtr display, WindowX w);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XNextEvent", ExactSpelling = true)]
		public static extern int XNextEvent(IntPtr display, XEvent* event_return);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XPeekEvent", ExactSpelling = true)]
		public static extern int XPeekEvent(IntPtr display, XEvent* event_return);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XSendEvent", ExactSpelling = true)]
		public static extern Status XSendEvent(IntPtr display, WindowX w, Bool propagate, Long event_mask, XEvent* event_send);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XPending", ExactSpelling = true)]
		public static extern int XPending(IntPtr display);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XWindowEvent", ExactSpelling = true)]
		public static extern int XWindowEvent(IntPtr display, WindowX w, Long event_mask, XEvent* event_return);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XFlush", ExactSpelling = true)]
		public static extern int XFlush(IntPtr display);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XInternAtom", ExactSpelling = true)]
		public static extern Atom XInternAtom(IntPtr display, byte* atom_name, Bool only_if_exists);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XChangeProperty", ExactSpelling = true)]
		public static extern int XChangeProperty(IntPtr display, WindowX window, Atom property, Atom type, int format, int mode, byte* data, int nelements);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XSetWMProtocols", ExactSpelling = true)]
		public static extern Status XSetWMProtocols(IntPtr display, WindowX w, Atom* protocols, int count);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XMoveWindow", ExactSpelling = true)]
		public static extern int XMoveWindow(IntPtr display, WindowX w, int x, int y);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XResizeWindow", ExactSpelling = true)]
		public static extern int XResizeWindow(IntPtr display, WindowX w, uint width, uint height);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XWarpPointer", ExactSpelling = true)]
		public static extern int XWarpPointer(IntPtr display, WindowX src_w, WindowX dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XQueryPointer", ExactSpelling = true)]
		public static extern Bool XQueryPointer(IntPtr display, WindowX w, WindowX* root_return, WindowX* child_return, int* root_x_return, int* root_y_return, int* win_x_return, int* win_y_return, uint* mask_return);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(DLL, EntryPoint = "XStoreName", ExactSpelling = true)]
		public static extern int XStoreName(IntPtr display, WindowX w, byte* name);
		
		/// <summary>
		/// Extensions
		/// </summary>
		public static class Ext
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct _MOTIF_WM_HINTS
			{
				public uint flags;
				public uint functions;
				public uint decorations;
				public int input_mode;
				public uint status;
			};
		
			public enum _MOTIF_WM_HINTS__FLAGS : uint
			{
				FUNCTIONS = (1u << 0),
				DECORATIONS =  (1u << 1)
			}
		
			public enum _MOTIF_WM_HINTS__FUNCTIONS : uint
			{
				ALL = (1u << 0),
				RESIZE = (1u << 1),
				MOVE = (1u << 2),
				MINIMIZE = (1u << 3),
				MAXIMIZE = (1u << 4),
				CLOSE = (1u << 5)
			}
		}
	}
}

