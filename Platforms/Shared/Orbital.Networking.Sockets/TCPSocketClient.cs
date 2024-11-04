using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
    public class TCPSocketClient : TCPSocket
    {
		public TCPSocketConnection connection {get; private set;}
		private NativeSocket nativeSocket;
		private readonly IPAddress localAddress;
		private readonly int timeout;
		private readonly bool bindLocalEndPoint;

		private Thread thread;
		private bool threadAlive;
		private bool connectAsync;

        /// <summary>
        /// TCPSocketClient
        /// </summary>
        /// <param name="remoteAddress">Remote address we want to connect to</param>
        /// <param name="localAddress">Local endPoint we want to connect from</param>
        /// <param name="port">Remote port we want to connect to</param>
        /// <param name="async">Use threads</param>
		/// <param name="connectAsync">Use threads to make connection</param>
		/// <param name="timeout">Timeout in seconds (default no timeout)</param>
        /// <param name="bindLocalEndPoint">Bind local endPoint address</param>
        public TCPSocketClient(IPAddress remoteAddress, IPAddress localAddress, int port, bool async, bool connectAsync, int timeout = -1, bool bindLocalEndPoint = false)
		: base(remoteAddress, port, async)
		{
			this.localAddress = localAddress;
			this.connectAsync = connectAsync;
			this.timeout = timeout;
			this.bindLocalEndPoint = bindLocalEndPoint;
		}

		public override void Dispose()
		{
			lock (this)
			{
				base.Dispose();

				// dispose native socket if connection object doesn't exist
				if (nativeSocket != null) TCPSocketConnection.Dispose(nativeSocket);
				nativeSocket = null;
			}
		}

        /// <summary>
        /// Start attempting a connection
        /// </summary>
        /// <returns>Connection if not in async mode, otherwise null</returns>
        public TCPSocketConnection Connect()
		{
			lock (this)
			{
				if (isDisposed || nativeSocket != null) throw new Exception("Can only be called once");
				nativeSocket = new NativeSocket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				if (bindLocalEndPoint && !IPAddress.IsLoopback(localAddress)) nativeSocket.Bind(new IPEndPoint(localAddress, 0));// NOTE: binding to a port other than zero will have to wait for WAIT_TIME to timeout

				if (connectAsync)
				{
					thread = new Thread(AsyncThread);
					thread.IsBackground = true;
					threadAlive = true;
					thread.Start();
					return null;
				}
				else
				{
					return ConnectSync(true);
                }
			}
		}

		private TCPSocketConnection ConnectSync(bool invokeCallback)
		{
            nativeSocket.Connect(address, port);
            nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endPoint to be reused if Windows fails to close it on app quit
            nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
            if (timeout >= 0) nativeSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout * 1000);

            var remoteEndPoint = nativeSocket.RemoteEndPoint as IPEndPoint;
            var localEndPoint = nativeSocket.LocalEndPoint as IPEndPoint;
            if (remoteEndPoint == null) remoteEndPoint = new IPEndPoint(IPAddress.None, port);
            if (localEndPoint == null) localEndPoint = new IPEndPoint(IPAddress.None, port);
            lock (this)
            {
				if (isDisposed) return null;
                var connection = new TCPSocketConnection(this, nativeSocket, remoteEndPoint.Address, port, localEndPoint.Address, async);
                _connections.Add(connection);
                connection.Init();
				if (invokeCallback) InvokeConnectedCallback(this, connection, true, null);
				return connection;
            }
        }

		private void AsyncThread(object obj)
		{
			if (threadAlive && !isDisposed)
			{
				TCPSocketConnection connection = null;

				try
				{
					connection = ConnectSync(false);
				}
				catch (Exception e)
				{
					if (connection != null) connection.Dispose();
					InvokeConnectedCallback(this, connection, false, e.Message);
					goto FINISH;
				}

				InvokeConnectedCallback(this, connection, true, null);
			}

			FINISH:
			threadAlive = false;
		}

		internal override void RemoveConnection(TCPSocketConnection connection)
		{
			lock (this)
			{
				base.RemoveConnection(connection);
				this.connection = null;
				nativeSocket = null;
			}
		}
	}
}
