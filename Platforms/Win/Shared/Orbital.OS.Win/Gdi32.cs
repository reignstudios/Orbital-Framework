using System.Runtime.InteropServices;

using HDC = System.IntPtr;

namespace Orbital.OS.Win
{
	public static class Gdi32
	{
		public const string lib = "Gdi32.dll";

		[DllImport(lib, EntryPoint = "CreateCompatibleDC")]
		public static extern HDC CreateCompatibleDC(HDC hdc);
	}
}
