using System;

namespace Orbital.Video
{
	public abstract class ConstantBufferBase : IDisposable
	{
		public int size { get; protected set; }

		public abstract void Dispose();
	}
}
