using System;
using System.Net;
using System.Net.Sockets;

using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
    public class TCPSocketClient : TCPSocket
    {
		public TCPSocketConnection connection {get; private set;}
		private NativeSocket nativeSocket;
		private readonly IPAddress localAddress;
		private readonly int timeout;

		public TCPSocketClient(IPAddress remoteAddress, IPAddress localAddress, int port, int timeout = -1, bool async = true)
		: base(remoteAddress, port, async)
		{
			this.localAddress = localAddress;
			this.timeout = timeout;
		}

		public override void Dispose()
		{
			isDisposed = true;

			TCPSocketConnection connectionObj;
			lock (this)
			{
				if (connection == null && nativeSocket != null) TCPSocketConnection.Dispose(nativeSocket);
				nativeSocket = null;

				connectionObj = connection;
				connection = null;
			}

			if (connectionObj != null) connectionObj.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// Start attempting a connection
		/// </summary>
		public void Connect()
		{
			lock (this)
			{
				if (isDisposed || nativeSocket != null) throw new Exception("Can only be called once");
				nativeSocket = new NativeSocket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endpoint to be reused if Windows fails to close it on app quit
				nativeSocket.Blocking = true;
				if (timeout >= 0)
				{
					nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
					nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout * 1000);
					//nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout * 1000);// not needed
				}
				if (!IPAddress.IsLoopback(localAddress) && !IPAddress.Any.Equals(localAddress)) nativeSocket.Bind(new IPEndPoint(localAddress, port));
				nativeSocket.BeginConnect(new IPEndPoint(address, port), ConnectionCallback, null);
			}
		}

		private void ConnectionCallback(IAsyncResult ar)
		{
			bool success = false;
			string message = null;

			// connect
			lock (this)
			{
				if (isDisposed) return;
				
				try
				{
					nativeSocket.EndConnect(ar);
					if (nativeSocket.Connected)
					{
						var localEndPoint = nativeSocket.LocalEndPoint as IPEndPoint;
						if (localEndPoint == null) localEndPoint = new IPEndPoint(IPAddress.None, port);
						connection = new TCPSocketConnection(this, nativeSocket, address, port, localEndPoint.Address, async);
						connection = connection;
						_connections.Add(connection);
						success = true;
					}
				}
				catch (SocketException e)
				{
					success = false;
					message = string.Format("socket.EndConnect failed: {0}\n{1}", e.SocketErrorCode, e.Message);
				}
				catch (Exception e)
				{
					success = false;
					message = "socket.EndConnect failed: " + e.Message;
				}
				finally
				{
					if (!success && connection != null)
					{
						connection.Dispose(message);
						connection = null;
					}
				}
			}

			// fire connected callback
			FireConnectedCallback(this, connection, success, message);

			// start recieving data
			if (async)
			{
				bool disconnected = false;
				lock (this)
				{
					if (success && !isDisposed)
					{
						try
						{
							connection.InitRecieve();
						}
						catch
						{
							disconnected = true;
						}
					}
				}

				if (disconnected) connection.Dispose("Disconnected while trying to connect");
			}
		}

		internal override void RemoveConnection(TCPSocketConnection connection)
		{
			base.RemoveConnection(connection);
			lock (this)
			{
				if (!isDisposed && this.connection != connection) throw new Exception("Connection objects don't match (this should never happen)");
				this.connection = null;
				this.nativeSocket = null;
			}
		}
	}
}
