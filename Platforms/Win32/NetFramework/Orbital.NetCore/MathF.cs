#region Assembly System.Runtime.Extensions, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\3.1.0\ref\netcoreapp3.1\System.Runtime.Extensions.dll
#endregion

using System;

namespace System
{
	//
	// Summary:
	//     Provides constants and static methods for trigonometric, logarithmic, and other
	//     common mathematical functions.
	public static class MathF
	{
		//
		// Summary:
		//     Represents the natural logarithmic base, specified by the constant, e.
		public const float E = 2.71828183f;
		//
		// Summary:
		//     Represents the ratio of the circumference of a circle to its diameter, specified
		//     by the constant, π.
		public const float PI = 3.14159265f;

		//
		// Summary:
		//     Returns the absolute value of a single-precision floating-point number.
		//
		// Parameters:
		//   x:
		//     A number that is greater than or equal to System.Single.MinValue, but less than
		//     or equal to System.Single.MaxValue.
		//
		// Returns:
		//     A single-precision floating-point number, x, such that 0 ≤ x ≤ System.Single.MaxValue.
		public static float Abs(float x)
		{
			return Math.Abs(x);
		}

		//
		// Summary:
		//     Returns the angle whose cosine is the specified number.
		//
		// Parameters:
		//   x:
		//     A number representing a cosine, where x must be greater than or equal to -1,
		//     but less than or equal to 1.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that 0 ≤ θ ≤ π. -or- System.Single.NaN
		//     if x < -1 or x > 1 or x equals System.Single.NaN.
		public static float Acos(float x)
		{
			return (float)Math.Acos(x);
		}

		//
		// Summary:
		//     Returns the angle whose hyperbolic cosine is the specified number.
		//
		// Parameters:
		//   x:
		//     A number representing a hyperbolic cosine, where x must be greater than or equal
		//     to 1, but less than or equal to System.Single.PositiveInfinity.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that 0 ≤ θ ≤ ∞. -or- System.Single.NaN
		//     if x < 1 or x equals System.Single.NaN.
		public static float Acosh(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.Acosh(x);
		}

		//
		// Summary:
		//     Returns the angle whose sine is the specified number.
		//
		// Parameters:
		//   x:
		//     A number representing a sine, where x must be greater than or equal to -1, but
		//     less than or equal to 1.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. -or- System.Single.NaN
		//     if x < -1 or x > 1 or x equals System.Single.NaN.
		public static float Asin(float x)
		{
			return (float)Math.Asin(x);
		}

		//
		// Summary:
		//     Returns the angle whose hyperbolic sine is the specified number.
		//
		// Parameters:
		//   x:
		//     A number representing a hyperbolic sine, where x must be greater than or equal
		//     to System.Single.NegativeInfinity, but less than or equal to System.Single.PositiveInfinity.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that -∞ < θ ≤ -1, or 1 ≤ θ < ∞. -or- System.Single.NaN
		//     if x equals System.Single.NaN.
		public static float Asinh(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.Asinh(x);
		}

		//
		// Summary:
		//     Returns the angle whose tangent is the specified number.
		//
		// Parameters:
		//   x:
		//     A number representing a tangent.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. -or- System.Single.NaN
		//     if x equals System.Single.NaN, -π/2 rounded to double precision (-1.5707963267949)
		//     if x equals System.Single.NegativeInfinity, or π/2 rounded to double precision
		//     (1.5707963267949) if x equals System.Single.PositiveInfinity.
		public static float Atan(float x)
		{
			return (float)Math.Atan(x);
		}

		//
		// Summary:
		//     Returns the angle whose tangent is the quotient of two specified numbers.
		//
		// Parameters:
		//   y:
		//     The y coordinate of a point.
		//
		//   x:
		//     The x coordinate of a point.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that -π ≤ θ ≤ π, and tan(θ) = y / x, where
		//     (x, y) is a point in the Cartesian plane. Observe the following: For (x, y) in
		//     quadrant 1, 0 < θ < π/2. For (x, y) in quadrant 2, π/2 < θ ≤ π. For (x, y) in
		//     quadrant 3, -π < θ < -π/2. For (x, y) in quadrant 4, -π/2 < θ < 0. For points
		//     on the boundaries of the quadrants, the return value is the following: If y is
		//     0 and x is not negative, θ = 0. If y is 0 and x is negative, θ = π. If y is positive
		//     and x is 0, θ = π/2. If y is negative and x is 0, θ = -π/2. If y is 0 and x is
		//     0, θ = 0. If x or y is System.Single.NaN, or if x and y are either System.Single.PositiveInfinity
		//     or System.Single.NegativeInfinity, the method returns System.Single.NaN.
		public static float Atan2(float y, float x)
		{
			return (float)Math.Atan2(y, x);
		}

		//
		// Summary:
		//     Returns the angle whose hyperbolic tangent is the specified number.
		//
		// Parameters:
		//   x:
		//     A number representing a hyperbolic tangent, where x must be greater than or equal
		//     to -1, but less than or equal to 1.
		//
		// Returns:
		//     An angle, θ, measured in radians, such that -∞ < θ <-1, or 1 < θ < ∞. -or- System.Single.NaN
		//     if x < -1 or x > 1 or x equals System.Single.NaN.
		public static float Atanh(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.Atanh(x);
		}

		//
		// Summary:
		//     Returns the next smallest value that compares less than x.
		//
		// Parameters:
		//   x:
		//     The value to decrement.
		//
		// Returns:
		//     The next smallest value that compares less than x. -or- System.Single.NegativeInfinity
		//     if x is equals System.Single.NegativeInfinity. -or- System.Single.NaN if x equals
		//     System.Single.NaN.
		public static float BitDecrement(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.BitDecrement(x);
		}

		//
		// Summary:
		//     Returns the next largest value that is greater than x.
		//
		// Parameters:
		//   x:
		//     The value to increment.
		//
		// Returns:
		//     The next largest value that is greater than x. -or- System.Single.PositiveInfinity
		//     if x equals System.Single.PositiveInfinity. -or- System.Single.NaN if x is equals
		//     System.Single.NaN.
		public static float BitIncrement(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.BitIncrement(x);
		}

		//
		// Summary:
		//     Returns the cube root of a specified number.
		//
		// Parameters:
		//   x:
		//     The number whose cube root is to be found.
		//
		// Returns:
		//     The cube root of x. -or- System.Single.NaN if x is equals System.Single.NaN.
		public static float Cbrt(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.Cbrt(x);
		}

		//
		// Summary:
		//     Returns the smallest integral value that is greater than or equal to the specified
		//     single-precision floating-point number.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number.
		//
		// Returns:
		//     The smallest integral value that is greater than or equal to x. If x is equal
		//     to System.Single.NaN, System.Single.NegativeInfinity, or System.Single.PositiveInfinity,
		//     that value is returned. Note that this method returns a System.Single instead
		//     of an integral type.
		public static float Ceiling(float x)
		{
			return (float)Math.Ceiling(x);
		}

		//
		// Summary:
		//     Returns a value with the magnitude of x and the sign of y.
		//
		// Parameters:
		//   x:
		//     A number whose magnitude is used in the result.
		//
		//   y:
		//     A number whose sign is the used in the result.
		//
		// Returns:
		//     A value with the magnitude of x and the sign of y.
		public static float CopySign(float x, float y)
		{
			throw new NotImplementedException();
			//return (float)Math.CopySign(x, y);
		}

		//
		// Summary:
		//     Returns the cosine of the specified angle.
		//
		// Parameters:
		//   x:
		//     An angle, measured in radians.
		//
		// Returns:
		//     The cosine of x. If x is equal to System.Single.NaN, System.Single.NegativeInfinity,
		//     or System.Single.PositiveInfinity, this method returns System.Single.NaN.
		public static float Cos(float x)
		{
			return (float)Math.Cos(x);
		}

		//
		// Summary:
		//     Returns the hyperbolic cosine of the specified angle.
		//
		// Parameters:
		//   x:
		//     An angle, measured in radians.
		//
		// Returns:
		//     The hyperbolic cosine of x. If x is equal to System.Single.NegativeInfinity or
		//     System.Single.PositiveInfinity, System.Single.PositiveInfinity is returned. If
		//     x is equal to System.Single.NaN, System.Single.NaN is returned.
		public static float Cosh(float x)
		{
			return (float)Math.Cosh(x);
		}

		//
		// Summary:
		//     Returns e raised to the specified power.
		//
		// Parameters:
		//   x:
		//     A number specifying a power.
		//
		// Returns:
		//     The number e raised to the power x. If x equals System.Single.NaN or System.Single.PositiveInfinity,
		//     that value is returned. If x equals System.Single.NegativeInfinity, 0 is returned.
		public static float Exp(float x)
		{
			return (float)Math.Exp(x);
		}

		//
		// Summary:
		//     Returns the largest integral value less than or equal to the specified single-precision
		//     floating-point number.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number.
		//
		// Returns:
		//     The largest integral value less than or equal to x. If x is equal to System.Single.NaN,
		//     System.Single.NegativeInfinity, or System.Single.PositiveInfinity, that value
		//     is returned.
		public static float Floor(float x)
		{
			return (float)Math.Floor(x);
		}

		//
		// Summary:
		//     Returns (x * y) + z, rounded as one ternary operation.
		//
		// Parameters:
		//   x:
		//     The number to be multiplied with y.
		//
		//   y:
		//     The number to be multiplied with x.
		//
		//   z:
		//     The number to be added to the result of x multiplied by y.
		//
		// Returns:
		//     (x * y) + z, rounded as one ternary operation.
		public static float FusedMultiplyAdd(float x, float y, float z)
		{
			throw new NotImplementedException();
			//return (float)Math.FusedMultiplyAdd(x, y, z);
		}

		//
		// Summary:
		//     Returns the remainder resulting from the division of a specified number by another
		//     specified number.
		//
		// Parameters:
		//   x:
		//     A dividend.
		//
		//   y:
		//     A divisor.
		//
		// Returns:
		//     A number equal to x - (y Q), where Q is the quotient of x / y rounded to the
		//     nearest integer (if x / y falls halfway between two integers, the even integer
		//     is returned). If x - (y Q) is zero, the value +0 is returned if x is positive,
		//     or -0 if x is negative. If y = 0, System.Single.NaN is returned.
		public static float IEEERemainder(float x, float y)
		{
			return (float)Math.IEEERemainder(x, y);
		}

		//
		// Summary:
		//     Returns the base 2 integer logarithm of a specified number.
		//
		// Parameters:
		//   x:
		//     The number whose logarithm is to be found.
		//
		// Returns:
		//     One of the values in the following table. x parameter Return value Default The
		//     base 2 integer log of x; that is, (int)log2(x). Zero System.Int32.MinValue Equal
		//     to System.Single.NaN or System.Single.PositiveInfinity or System.Single.NegativeInfinitySystem.Int32.MaxValue
		public static int ILogB(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.ILogB(x);
		}

		//
		// Summary:
		//     Returns the natural (base e) logarithm of a specified number.
		//
		// Parameters:
		//   x:
		//     The number whose logarithm is to be found.
		//
		// Returns:
		//     One of the values in the following table. x parameter Return value Positive The
		//     natural logarithm of x; that is, ln x, or log e x Zero System.Single.NegativeInfinity
		//     Negative System.Single.NaN Equal to System.Single.NaNSystem.Single.NaN Equal
		//     to System.Single.PositiveInfinitySystem.Single.PositiveInfinity
		public static float Log(float x)
		{
			return (float)Math.Log(x);
		}

		//
		// Summary:
		//     Returns the logarithm of a specified number in a specified base.
		//
		// Parameters:
		//   x:
		//     The number whose logarithm is to be found.
		//
		//   y:
		//     The base.
		//
		// Returns:
		//     One of the values in the following table. (+Infinity denotes System.Single.PositiveInfinity,
		//     -Infinity denotes System.Single.NegativeInfinity, and NaN denotes System.Single.NaN.)
		//     xnewBase Return value x> 0 (0 <newBase< 1) -or-(newBase> 1) lognewBase(a) x<
		//     0 (any value) NaN (any value) newBase< 0 NaN x != 1 newBase = 0 NaN x != 1 newBase
		//     = +Infinity NaN x = NaN (any value) NaN (any value) newBase = NaN NaN (any value)
		//     newBase = 1 NaN x = 0 0 <newBase< 1 +Infinity x = 0 newBase> 1 -Infinity x =
		//     +Infinity 0 <newBase< 1 -Infinity x = +Infinity newBase> 1 +Infinity x = 1 newBase
		//     = 0 0 x = 1 newBase = +Infinity 0
		public static float Log(float x, float y)
		{
			return (float)Math.Log(x, y);
		}

		//
		// Summary:
		//     Returns the base 10 logarithm of a specified number.
		//
		// Parameters:
		//   x:
		//     A number whose logarithm is to be found.
		//
		// Returns:
		//     One of the values in the following table. x parameter Return value Positive The
		//     base 10 log of x; that is, log 10x. Zero System.Single.NegativeInfinity Negative
		//     System.Single.NaN Equal to System.Single.NaNSystem.Single.NaN Equal to System.Single.PositiveInfinitySystem.Single.PositiveInfinity
		public static float Log10(float x)
		{
			return (float)Math.Log10(x);
		}

		//
		// Summary:
		//     Returns the base 2 logarithm of a specified number.
		//
		// Parameters:
		//   x:
		//     A number whose logarithm is to be found.
		//
		// Returns:
		//     One of the values in the following table. x parameter Return value Positive The
		//     base 2 log of x; that is, log 2x. Zero System.Single.NegativeInfinity Negative
		//     System.Single.NaN Equal to System.Single.NaNSystem.Single.NaN Equal to System.Single.PositiveInfinitySystem.Single.PositiveInfinity
		public static float Log2(float x)
		{
			throw new NotImplementedException();
			//return (float)Math.Log2(x);
		}

		//
		// Summary:
		//     Returns the larger of two single-precision floating-point numbers.
		//
		// Parameters:
		//   x:
		//     The first of two single-precision floating-point numbers to compare.
		//
		//   y:
		//     The second of two single-precision floating-point numbers to compare.
		//
		// Returns:
		//     Parameter x or y, whichever is larger. If x, or y, or both x and y are equal
		//     to System.Single.NaN, System.Single.NaN is returned.
		public static float Max(float x, float y)
		{
			return Math.Max(x, y);
		}

		//
		// Summary:
		//     Returns the larger magnitude of two single-precision floating-point numbers.
		//
		// Parameters:
		//   x:
		//     The first of two single-precision floating-point numbers to compare.
		//
		//   y:
		//     The second of two single-precision floating-point numbers to compare.
		//
		// Returns:
		//     Parameter x or y, whichever has the larger magnitude. If x, or y, or both x and
		//     y are equal to System.Single.NaN, System.Single.NaN is returned.
		public static float MaxMagnitude(float x, float y)
		{
			throw new NotImplementedException();
			//return Math.MaxMagnitude(x, y);
		}

		//
		// Summary:
		//     Returns the smaller of two single-precision floating-point numbers.
		//
		// Parameters:
		//   x:
		//     The first of two single-precision floating-point numbers to compare.
		//
		//   y:
		//     The second of two single-precision floating-point numbers to compare.
		//
		// Returns:
		//     Parameter x or y, whichever is smaller. If x, y, or both x and y are equal to
		//     System.Single.NaN, System.Single.NaN is returned.
		public static float Min(float x, float y)
		{
			return Math.Min(x, y);
		}

		//
		// Summary:
		//     Returns the smaller magnitude of two single-precision floating-point numbers.
		//
		// Parameters:
		//   x:
		//     The first of two single-precision floating-point numbers to compare.
		//
		//   y:
		//     The second of two single-precision floating-point numbers to compare.
		//
		// Returns:
		//     Parameter x or y, whichever has the smaller magnitude. If x, or y, or both x
		//     and y are equal to System.Single.NaN, System.Single.NaN is returned.
		public static float MinMagnitude(float x, float y)
		{
			throw new NotImplementedException();
			//return Math.MinMagnitude(x, y);
		}

		//
		// Summary:
		//     Returns a specified number raised to the specified power.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number to be raised to a power.
		//
		//   y:
		//     A single-precision floating-point number that specifies a power.
		//
		// Returns:
		//     The number x raised to the power y.
		public static float Pow(float x, float y)
		{
			return (float)Math.Pow(x, y);
		}

		//
		// Summary:
		//     Rounds a single-precision floating-point value to the nearest integer, and uses
		//     the specified rounding convention for midpoint values.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number to be rounded.
		//
		//   mode:
		//     Specification for how to round x if it is midway between two other numbers.
		//
		// Returns:
		//     The integer nearest x. If x is halfway between two integers, one of which is
		//     even and the other odd, then mode determines which of the two is returned. Note
		//     that this method returns a System.Single instead of an integral type.
		//
		// Exceptions:
		//   T:System.ArgumentException:
		//     mode is not a valid value of System.MidpointRounding.
		public static float Round(float x, MidpointRounding mode)
		{
			return (float)Math.Round(x, mode);
		}

		//
		// Summary:
		//     Rounds a single-precision floating-point value to a specified number of fractional
		//     digits, and uses the specified rounding convention for midpoint values.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number to be rounded.
		//
		//   digits:
		//     The number of fractional digits in the return value.
		//
		//   mode:
		//     Specification for how to round x if it is midway between two other numbers.
		//
		// Returns:
		//     The number nearest to x that has a number of fractional digits equal to digits.
		//     If x has fewer fractional digits than digits, x is returned unchanged.
		//
		// Exceptions:
		//   T:System.ArgumentOutOfRangeException:
		//     digits is less than 0 or greater than 6.
		//
		//   T:System.ArgumentException:
		//     mode is not a valid value of System.MidpointRounding.
		public static float Round(float x, int digits, MidpointRounding mode)
		{
			return (float)Math.Round(x, digits, mode);
		}

		//
		// Summary:
		//     Rounds a single-precision floating-point value to a specified number of fractional
		//     digits, and rounds midpoint values to the nearest even number.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number to be rounded.
		//
		//   digits:
		//     The number of fractional digits in the return value.
		//
		// Returns:
		//     The number nearest to x that contains a number of fractional digits equal to
		//     digits.
		//
		// Exceptions:
		//   T:System.ArgumentOutOfRangeException:
		//     digits is less than 0 or greater than 6.
		public static float Round(float x, int digits)
		{
			return (float)Math.Round(x, digits);
		}

		//
		// Summary:
		//     Rounds a single-precision floating-point value to the nearest integral value,
		//     and rounds midpoint values to the nearest even number.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number to be rounded.
		//
		// Returns:
		//     The integer nearest x. If the fractional component of x is halfway between two
		//     integers, one of which is even and the other odd, then the even number is returned.
		//     Note that this method returns a System.Single instead of an integral type.
		public static float Round(float x)
		{
			return (float)Math.Round(x);
		}

		//
		// Summary:
		//     Returns x * 2^n computed efficiently.
		//
		// Parameters:
		//   x:
		//     A single-precision floating-point number that specifies the base value.
		//
		//   n:
		//     A single-precision floating-point number that specifies the power.
		//
		// Returns:
		//     x * 2^n computed efficiently.
		public static float ScaleB(float x, int n)
		{
			throw new NotImplementedException();
			//return (float)Math.ScaleB(x, n);
		}

		//
		// Summary:
		//     Returns an integer that indicates the sign of a single-precision floating-point
		//     number.
		//
		// Parameters:
		//   x:
		//     A signed number.
		//
		// Returns:
		//     A number that indicates the sign of x, as shown in the following table. Return
		//     value Meaning -1 x is less than zero. 0 x is equal to zero. 1 x is greater than
		//     zero.
		//
		// Exceptions:
		//   T:System.ArithmeticException:
		//     x is equal to System.Single.NaN.
		public static int Sign(float x)
		{
			return Math.Sign(x);
		}

		//
		// Summary:
		//     Returns the sine of the specified angle.
		//
		// Parameters:
		//   x:
		//     An angle, measured in radians.
		//
		// Returns:
		//     The sine of x. If x is equal to System.Single.NaN, System.Single.NegativeInfinity,
		//     or System.Single.PositiveInfinity, this method returns System.Single.NaN.
		public static float Sin(float x)
		{
			return (float)Math.Sin(x);
		}

		//
		// Summary:
		//     Returns the hyperbolic sine of the specified angle.
		//
		// Parameters:
		//   x:
		//     An angle, measured in radians.
		//
		// Returns:
		//     The hyperbolic sine of x. If x is equal to System.Single.NegativeInfinity, System.Single.PositiveInfinity,
		//     or System.Single.NaN, this method returns a System.Single equal to x.
		public static float Sinh(float x)
		{
			return (float)Math.Sinh(x);
		}

		//
		// Summary:
		//     Returns the square root of a specified number.
		//
		// Parameters:
		//   x:
		//     The number whose square root is to be found.
		//
		// Returns:
		//     One of the values in the following table. x parameter Return value Zero or positive
		//     The positive square root of x. Negative System.Single.NaN Equals System.Single.NaNSystem.Single.NaN
		//     Equals System.Single.PositiveInfinitySystem.Single.PositiveInfinity
		public static float Sqrt(float x)
		{
			return (float)Math.Sqrt(x);
		}

		//
		// Summary:
		//     Returns the tangent of the specified angle.
		//
		// Parameters:
		//   x:
		//     An angle, measured in radians.
		//
		// Returns:
		//     The tangent of x. If x is equal to System.Single.NaN, System.Single.NegativeInfinity,
		//     or System.Single.PositiveInfinity, this method returns System.Single.NaN.
		public static float Tan(float x)
		{
			return (float)Math.Tan(x);
		}

		//
		// Summary:
		//     Returns the hyperbolic tangent of the specified angle.
		//
		// Parameters:
		//   x:
		//     An angle, measured in radians.
		//
		// Returns:
		//     The hyperbolic tangent of x. If x is equal to System.Single.NegativeInfinity,
		//     this method returns -1. If value is equal to System.Single.PositiveInfinity,
		//     this method returns 1. If x is equal to System.Single.NaN, this method returns
		//     System.Single.NaN.
		public static float Tanh(float x)
		{
			return (float)Math.Tanh(x);
		}

		//
		// Summary:
		//     Calculates the integral part of a specified single-precision floating-point number.
		//
		// Parameters:
		//   x:
		//     A number to truncate.
		//
		// Returns:
		//     The integral part of x; that is, the number that remains after any fractional
		//     digits have been discarded, or one of the values listed in the following table.
		//     x Return value System.Single.NaNSystem.Single.NaNSystem.Single.NegativeInfinitySystem.Single.NegativeInfinitySystem.Single.PositiveInfinitySystem.Single.PositiveInfinity
		public static float Truncate(float x)
		{
			return (float)Math.Truncate(x);
		}
	}
}