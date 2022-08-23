using System;
using System.Net;

namespace Orbital.Networking.Sockets
{
    public abstract class Socket : IDisposable
    {
		public readonly IPAddress address;
		public readonly int port;
		protected bool isDisposed;

		public Socket(IPAddress address, int port)
		{
			this.address = address;
			this.port = port;
		}

		public abstract void Dispose();
		public abstract bool IsConnected();
    }
}
