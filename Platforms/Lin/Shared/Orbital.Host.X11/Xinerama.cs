using System.Runtime.InteropServices;
using System.Security;

using Bool = System.Int32;
using Status = System.Int32;

namespace Orbital.Host.X11
{
	public unsafe static class Xinerama
	{
		public const string XineramaDLL = "libXinerama.so.1";
		
		[StructLayout(LayoutKind.Sequential)]
		public struct XineramaScreenInfo
		{
			public int screen_number;
			public short x_org;
			public short y_org;
			public short width;
			public short height;
		}
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(XineramaDLL, EntryPoint = "XInitThreads", ExactSpelling = true)]
		public static extern Status XInitThreads();
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(XineramaDLL, EntryPoint = "XineramaQueryScreens", ExactSpelling = true)]
		public static extern XineramaScreenInfo* XineramaQueryScreens(IntPtr dpy, int* number);
		
		[SuppressUnmanagedCodeSecurity]
		[DllImport(XineramaDLL, EntryPoint = "XineramaIsActive", ExactSpelling = true)]
		public static extern Bool XineramaIsActive(IntPtr dpy);
	}
}
