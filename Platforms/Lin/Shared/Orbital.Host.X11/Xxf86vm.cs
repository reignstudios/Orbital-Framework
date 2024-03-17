using System.Runtime.InteropServices;
using System.Security;

using Bool = System.Int32;

namespace Orbital.Host.X11
{
	public unsafe static class Xxf86vm
	{
		public const string XF86DLL = "libXxf86vm.so";
		
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
		public static extern Bool XF86VidModeQueryVersion(IntPtr dpy, int* major_version_return, int* minor_version_return);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(XF86DLL, EntryPoint = "XF86VidModeGetAllModeLines")]
		public static extern Bool XF86VidModeGetAllModeLines(IntPtr dpy, int screen, int* modecount_return, XF86VidModeModeInfo*** modesinfo);
	}
}
