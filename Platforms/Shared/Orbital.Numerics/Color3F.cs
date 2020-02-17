using System;
using System.Runtime.InteropServices;

namespace Orbital.Numerics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Color3F
	{
		#region Properties
		public float r, g, b;

		public static readonly Color3F black = new Color3F(0, 0, 0);
		public static readonly Color3F white = new Color3F(1, 1, 1);
		public static readonly Color3F red = new Color3F(1, 0, 0);
		public static readonly Color3F green = new Color3F(0, 1, 0);
		public static readonly Color3F blue = new Color3F(0, 0, 1);
		#endregion

		#region Constructors
		public Color3F(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}
		#endregion

		#region Operators
		// convert
		public Vec3 ToVec3()
		{
			return new Vec3(r, g, b);
		}

		public Vec4 ToVec4()
		{
			return new Vec4(r, g, b, 1);
		}
		#endregion

		#region Methods
		public Color3F LinearToSRGB()
		{
			const float gamma = 1 / 2.2f;
			return new Color3F(MathF.Pow(r, gamma), MathF.Pow(g, gamma), MathF.Pow(b, gamma));
		}

		public Color3F SRGBToLinear()
		{
			const float gamma = 2.2f;
			return new Color3F(MathF.Pow(r, gamma), MathF.Pow(g, gamma), MathF.Pow(b, gamma));
		}
		#endregion
	}
}