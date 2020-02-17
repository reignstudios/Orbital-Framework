using System;
using System.Runtime.InteropServices;

namespace Orbital.Numerics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Color4F
	{
		#region Properties
		public float r, g, b, a;

		public static readonly Color4F black = new Color4F(0, 0, 0, 1);
		public static readonly Color4F white = new Color4F(1, 1, 1, 1);
		public static readonly Color4F red = new Color4F(1, 0, 0, 1);
		public static readonly Color4F green = new Color4F(0, 1, 0, 1);
		public static readonly Color4F blue = new Color4F(0, 0, 1, 1);
		#endregion

		#region Constructors
		public Color4F(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
		#endregion

		#region Operators
		// convert
		public Vec4 ToVec4()
		{
			return new Vec4(r, g, b, a);
		}
		#endregion

		#region Methods
		public Color4F LinearToSRGB()
		{
			const float gamma = 1 / 2.2f;
			return new Color4F(MathF.Pow(r, gamma), MathF.Pow(g, gamma), MathF.Pow(b, gamma), MathF.Pow(a, gamma));
		}

		public Color4F SRGBToLinear()
		{
			const float gamma = 2.2f;
			return new Color4F(MathF.Pow(r, gamma), MathF.Pow(g, gamma), MathF.Pow(b, gamma), MathF.Pow(a, gamma));
		}
		#endregion
	}
}