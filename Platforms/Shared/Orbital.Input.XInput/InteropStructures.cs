using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Orbital.Input.XInput
{
	static class Interop
	{
		public const ushort XINPUT_GAMEPAD_DPAD_UP = 0x0001;
		public const ushort XINPUT_GAMEPAD_DPAD_DOWN = 0x0002;
		public const ushort XINPUT_GAMEPAD_DPAD_LEFT = 0x0004;
		public const ushort XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008;
		public const ushort XINPUT_GAMEPAD_START = 0x0010;
		public const ushort XINPUT_GAMEPAD_BACK = 0x0020;
		public const ushort XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
		public const ushort XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080;
		public const ushort XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
		public const ushort XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
		public const ushort XINPUT_GAMEPAD_A = 0x1000;
		public const ushort XINPUT_GAMEPAD_B = 0x2000;
		public const ushort XINPUT_GAMEPAD_X = 0x4000;
		public const ushort XINPUT_GAMEPAD_Y = 0x8000;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct XINPUT_GAMEPAD
	{
		public ushort wButtons;
		public byte bLeftTrigger;
		public byte bRightTrigger;
		public short sThumbLX;
		public short sThumbLY;
		public short sThumbRX;
		public short sThumbRY;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct XINPUT_STATE
	{
		public DWORD dwPacketNumber;
		public XINPUT_GAMEPAD Gamepad;
	}
}
