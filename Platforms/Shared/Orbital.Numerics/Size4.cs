using System.Runtime.InteropServices;

namespace Orbital.Numerics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Size4
	{
		#region Properties
		public int width, height, depth, time;

		public static readonly Size4 zero = new Size4();
		#endregion
	}
}