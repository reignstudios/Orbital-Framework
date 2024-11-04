using System;
using System.Collections.Generic;
using System.Net;

namespace Orbital.Networking.Sockets
{
	public abstract class WebSocket : IDisposable
    {
		public readonly string address;
		protected bool isDisposed;
		internal readonly int timeout;

		protected List<WebSocketConnection> _connections;
		public IReadOnlyList<WebSocketConnection> connections {get {return _connections;}}

		public delegate void GeneralErrorCallbackMethod(WebSocket socket, Exception e);
		public event GeneralErrorCallbackMethod GeneralErrorCallback;

		public WebSocket(string address, int timeout)
		{
			this.address = address;
			this.timeout = timeout;
			_connections = new List<WebSocketConnection>();
		}

		public virtual void Dispose()
		{
			isDisposed = true;
			if (_connections != null)
			{
				for (int i = _connections.Count - 1; i != -1; --i) _connections[i].Dispose();
				_connections = null;
			}
			ConnectedCallback = null;
		}

		public virtual bool IsConnected()
		{
			return !isDisposed && connections != null && connections.Count != 0;
		}

		public delegate void ConnectedCallbackMethod(WebSocket socket, WebSocketConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;
		protected void InvokeConnectedCallback(WebSocket socket, WebSocketConnection connection, bool success, string message)
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

		internal virtual void RemoveConnection(WebSocketConnection connection)
		{
			if (_connections != null) _connections.Remove(connection);
		}

		protected void InvokeGeneralErrorCallback(WebSocket socket, Exception e)
		{
			GeneralErrorCallback?.Invoke(socket, e);
		}
	}
}
