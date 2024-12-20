﻿using System;
using System.Collections.Generic;
using System.Net;

namespace Orbital.Networking.Sockets
{
	public abstract class TCPSocket : Socket
    {
		protected List<TCPSocketConnection> _connections;
		public IReadOnlyList<TCPSocketConnection> connections {get {return _connections;}}
		protected readonly bool async;

		public delegate void GeneralErrorCallbackMethod(TCPSocket socket, Exception e);
		public event GeneralErrorCallbackMethod GeneralErrorCallback;

		public TCPSocket(IPAddress address, int port, bool async)
		: base(address, port)
		{
			this.async = async;
			_connections = new List<TCPSocketConnection>();
		}

		public override void Dispose()// NOTE: this should be called in lock in abstracting class
		{
			isDisposed = true;
			if (_connections != null)
			{
				for (int i = _connections.Count - 1; i != -1; --i) _connections[i].Dispose();
				_connections = null;
			}
			ConnectedCallback = null;
		}

		public override bool IsConnected()
		{
			lock (this) return !isDisposed && connections != null && connections.Count != 0;
		}

		public delegate void ConnectedCallbackMethod(TCPSocket socket, TCPSocketConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;
		protected void InvokeConnectedCallback(TCPSocket socket, TCPSocketConnection connection, bool success, string message)
		{
			try
			{
				ConnectedCallback?.Invoke(socket, connection, success, message);
			}
			catch (Exception e)
			{
				GeneralErrorCallback?.Invoke(this, e);
			}
		}

		internal virtual void RemoveConnection(TCPSocketConnection connection)
		{
			lock (this)
			{
				if (_connections != null) _connections.Remove(connection);
			}
		}

		protected void InvokeGeneralErrorCallback(TCPSocket socket, Exception e)
		{
			GeneralErrorCallback?.Invoke(socket, e);
		}
	}
}
