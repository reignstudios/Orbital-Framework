using System;

namespace Orbital.Video
{
	public enum ConstantBufferMode
	{
		/// <summary>
		/// Constant buffer memory will be frequently updated from the CPU
		/// </summary>
		Update,

		/// <summary>
		/// Constant buffer memory will only be initialized by the CPU once
		/// </summary>
		Static
	}

	public abstract class ConstantBufferBase : IDisposable
	{
		public int size { get; protected set; }

		public abstract void Dispose();
	}
}
