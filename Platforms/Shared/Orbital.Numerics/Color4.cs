using System;
using System.Runtime.InteropServices;

namespace Orbital.Numerics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Color4
	{
		#region Properties
		public byte r, g, b, a;

		public static readonly Color4 black = new Color4(0, 0, 0, 255);
		public static readonly Color4 white = new Color4(255, 255, 255, 255);
		public static readonly Color4 red = new Color4(255, 0, 0, 255);
		public static readonly Color4 green = new Color4(0, 255, 0, 255);
		public static readonly Color4 blue = new Color4(0, 0, 255, 255);

		public int Value
		{
			get
			{
				int color = r;
				color |= g << 8;
				color |= b << 16;
				color |= a << 24;
				return color;
			}
			set
			{
				r = (byte)(value & 0x000000FF);
				g = (byte)((value & 0x0000FF00) >> 8);
				b = (byte)((value & 0x00FF0000) >> 16);
				a = (byte)((value & 0xFF000000) >> 24);
			}
		}
		#endregion

		#region Constructors
		public Color4(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color4(int color)
		{
			r = (byte)(color & 0x000000FF);
			g = (byte)((color & 0x0000FF00) >> 8);
			b = (byte)((color & 0x00FF0000) >> 16);
			a = (byte)((color & 0xFF000000) >> 24);
		}
		#endregion

		#region Operators
		// convert
		public Vec4 ToVec4()
		{
			return new Vec4(r/255f, g/255f, b/255f, a/255f);
		}
		#endregion

		#region Methods
		public Color4 LinearToSRGB()
		{
			const float gamma = 1 / 2.2f;
			return new Color4((byte)Math.Pow(r, gamma), (byte)Math.Pow(r, gamma), (byte)Math.Pow(r, gamma), (byte)Math.Pow(r, gamma));
		}

		public Color4 SRGBToLinear()
		{
			const float gamma = 2.2f;
			return new Color4((byte)Math.Pow(r, gamma), (byte)Math.Pow(r, gamma), (byte)Math.Pow(r, gamma), (byte)Math.Pow(r, gamma));
		}
		#endregion
	}
}