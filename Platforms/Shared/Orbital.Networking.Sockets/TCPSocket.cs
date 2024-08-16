using System;
using System.Collections.Generic;
using System.Net;

namespace Orbital.Networking.Sockets
{
	public abstract class TCPSocket : Socket
    {
		protected List<TCPSocketConnection> _connections;
		public IReadOnlyList<TCPSocketConnection> connections {get {return _connections;}}
		protected readonly bool async;

		public TCPSocket(IPAddress address, int port, bool async)
		: base(address, port)
		{
			this.async = async;
			_connections = new List<TCPSocketConnection>();
		}

		public override void Dispose()
		{
			isDisposed = true;

			List<TCPSocketConnection> connectionsObj;
			lock (this)
			{
				connectionsObj = _connections;
				_connections = null;
			}

			if (connectionsObj != null)
			{
				for (int i = connectionsObj.Count - 1; i != -1; --i) connectionsObj[i].Dispose();
			}

			ConnectedCallback = null;
		}

		public override bool IsConnected()
		{
			lock (this) return !isDisposed && connections != null && connections.Count != 0;
		}

		public delegate void ConnectedCallbackMethod(TCPSocket socket, TCPSocketConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;
		protected void FireConnectedCallback(TCPSocket socket, TCPSocketConnection connection, bool success, string message)
		{
			try
			{
				ConnectedCallback?.Invoke(socket, connection, success, message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				System.Diagnostics.Debug.WriteLine(e);
			}
		}

		internal virtual void RemoveConnection(TCPSocketConnection connection)
		{
			lock (this)
			{
				if (_connections != null) _connections.Remove(connection);
			}
		}
	}
}
