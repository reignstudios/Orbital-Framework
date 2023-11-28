using System.Runtime.InteropServices;

using LONG = System.Int32;

namespace Orbital.OS.Win
{
	public static class Windows
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public LONG left;
			public LONG top;
			public LONG right;
			public LONG bottom;
		}
	}
}
