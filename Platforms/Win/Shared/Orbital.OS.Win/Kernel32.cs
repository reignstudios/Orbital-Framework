using System;
using System.Runtime.InteropServices;

using WORD = System.UInt16;
using DWORD = System.UInt32;
using HANDLE = System.IntPtr;
using LPSTR = System.IntPtr;

namespace Orbital.OS.Win
{
	public static unsafe class Kernel32
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct STARTUPINFOA
		{
			public DWORD cb;
			public LPSTR lpReserved;
			public LPSTR lpDesktop;
			public LPSTR lpTitle;
			public DWORD dwX;
			public DWORD dwY;
			public DWORD dwXSize;
			public DWORD dwYSize;
			public DWORD dwXCountChars;
			public DWORD dwYCountChars;
			public DWORD dwFillAttribute;
			public DWORD dwFlags;
			public WORD wShowWindow;
			public WORD cbReserved2;
			public IntPtr lpReserved2;
			public HANDLE hStdInput;
			public HANDLE hStdOutput;
			public HANDLE hStdError;
		}

		public const string lib = "Kernel32.dll";

		[DllImport(lib, EntryPoint = "GetStartupInfoA")]
		public static extern void GetStartupInfoA(STARTUPINFOA* lpStartupInfo);
	}
}
