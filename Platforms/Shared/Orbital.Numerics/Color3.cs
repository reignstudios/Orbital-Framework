using System;
using System.Runtime.InteropServices;

namespace Orbital.Numerics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Color3
	{
		#region Properties
		public byte r, g, b;

		public static readonly Color3 black = new Color3(0, 0, 0);
		public static readonly Color3 white = new Color3(255, 255, 255);
		public static readonly Color3 red = new Color3(255, 0, 0);
		public static readonly Color3 green = new Color3(0, 255, 0);
		public static readonly Color3 blue = new Color3(0, 0, 255);

		public int Value
		{
			get
			{
				int color = r;
				color |= g << 8;
				color |= b << 16;
				color |= 255 << 24;
				return color;
			}
			set
			{
				r = (byte)(value & 0x000000FF);
				g = (byte)((value & 0x0000FF00) >> 8);
				b = (byte)((value & 0x00FF0000) >> 16);
			}
		}
		#endregion

		#region Constructors
		public Color3(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public Color3(int color)
		{
			r = (byte)(color & 0x000000FF);
			g = (byte)((color & 0x0000FF00) >> 8);
			b = (byte)((color & 0x00FF0000) >> 16);
		}
		#endregion

		#region Operators
		// convert
		public Vec3 ToVec3()
		{
			return new Vec3(r/255f, g/255f, b/255f);
		}

		public Vec4 ToVec4()
		{
			return new Vec4(r/255f, g/255f, b/255f, 255f);
		}
		#endregion

		#region Methods
		public Color3 LinearToSRGB()
		{
			const float gamma = 1 / 2.2f;
			return new Color3
			(
				(byte)MathF.Min(MathF.Pow(r, gamma), 255.0f),
				(byte)MathF.Min(MathF.Pow(g, gamma), 255.0f),
				(byte)MathF.Min(MathF.Pow(b, gamma), 255.0f)
			);
		}

		public Color3 SRGBToLinear()
		{
			const float gamma = 2.2f;
			return new Color3
			(
				(byte)MathF.Min(MathF.Pow(r, gamma), 255.0f),
				(byte)MathF.Min(MathF.Pow(g, gamma), 255.0f),
				(byte)MathF.Min(MathF.Pow(b, gamma), 255.0f)
			);
		}
		#endregion
	}
}