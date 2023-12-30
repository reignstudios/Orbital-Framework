using System.Runtime.InteropServices;
using System.Security;

namespace Orbital.Host.X11
{
	public unsafe static class Xxf86vm
	{
		public const string XF86DLL = "libXxf86vm.so.1";
		
		[StructLayout(LayoutKind.Sequential)]
		public struct XF86VidModeModeInfo
		{
		    public uint	dotclock;
		    public ushort hdisplay;
		    public ushort hsyncstart;
		    public ushort hsyncend;
		    public ushort htotal;
		    public ushort hskew;
		    public ushort vdisplay;
		    public ushort vsyncstart;
		    public ushort vsyncend;
		    public ushort vtotal;
		    public uint	flags;
		    public int privsize;
		    public int c_private;
		}
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(XF86DLL, EntryPoint = "XF86VidModeQueryVersion", ExactSpelling = true)]
		public static extern bool XF86VidModeQueryVersion(IntPtr dpy, out int majorVersion, out int minorVersion);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(XF86DLL, EntryPoint = "XF86VidModeGetAllModeLines")]
		public unsafe static extern bool XF86VidModeGetAllModeLines(IntPtr dpy, int screen, out int modecount, XF86VidModeModeInfo*** modelinesPtr);
	}
}
