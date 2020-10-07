using System;
using Orbital.Numerics;

namespace Orbital.Video
{
	public enum CommandListType
	{
		Rasterize,
		Compute
	}

	public abstract class CommandListBase : IDisposable
	{
		public abstract void Dispose();
	}
}
