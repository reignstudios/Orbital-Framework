using System;
using System.Net;

namespace Orbital.Networking.Sockets
{
    public abstract class Socket : IDisposable
    {
		public readonly IPAddress address;
		public readonly int port;
		public readonly IPEndPoint endPoint;
		protected bool isDisposed;

		public Socket(IPAddress address, int port)
		{
			this.address = address;
			this.port = port;
			endPoint = new IPEndPoint(address, port);
		}

		public virtual void Dispose()
		{
			isDisposed = true;
		}

		public abstract bool IsConnected();
    }
}
