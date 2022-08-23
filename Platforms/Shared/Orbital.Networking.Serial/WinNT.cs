using System;
using System.Collections.Generic;
using System.Text;

namespace Orbital.OS.Native
{
	public static class WinNT
	{
		public const uint DELETE = 0x00010000;
		public const uint READ_CONTROL = 0x00020000;
		public const uint WRITE_DAC = 0x00040000;
		public const uint WRITE_OWNER = 0x00080000;
		public const uint SYNCHRONIZE = 0x00100000;

		public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;

		public const uint STANDARD_RIGHTS_READ = READ_CONTROL;
		public const uint STANDARD_RIGHTS_WRITE = READ_CONTROL;
		public const uint STANDARD_RIGHTS_EXECUTE = READ_CONTROL;

		public const uint STANDARD_RIGHTS_ALL = 0x001F0000;
		public const uint SPECIFIC_RIGHTS_ALL = 0x0000FFFF;

		public const uint KEY_QUERY_VALUE = 0x0001;
		public const uint KEY_SET_VALUE = 0x0002;
		public const uint KEY_CREATE_SUB_KEY = 0x0004;
		public const uint KEY_ENUMERATE_SUB_KEYS = 0x0008;
		public const uint KEY_NOTIFY = 0x0010;
		public const uint KEY_CREATE_LINK = 0x0020;
		public const uint KEY_WOW64_32KEY = 0x0200;
		public const uint KEY_WOW64_64KEY = 0x0100;
		public const uint KEY_WOW64_RES = 0x0300;

		public const uint KEY_READ = ((STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY) & (~SYNCHRONIZE));
		public const uint KEY_WRITE = ((STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY) & (~SYNCHRONIZE));
		public const uint KEY_EXECUTE = ((KEY_READ) & (~SYNCHRONIZE));
		public const uint KEY_ALL_ACCESS = ((STANDARD_RIGHTS_ALL | KEY_QUERY_VALUE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY | KEY_CREATE_LINK) & (~SYNCHRONIZE));
	}
}
