using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
    public class TCPSocketServer : TCPSocket
    {
		private bool isListening;
		private NativeSocket tcpListenSocket;
		private readonly int timeout;

		private Thread thread;
		private bool threadAlive, listenAsync;


        public delegate void ListenDisconnectedErrorCallbackMethod(TCPSocketServer socket, string message);
		public event ListenDisconnectedErrorCallbackMethod ListenDisconnectedErrorCallback;

		/// <summary>
		/// TCPSocketServer
		/// </summary>
		/// <param name="listenAddress">Address to listen for connection requests on</param>
		/// <param name="port">Port all traffic is sent over</param>
		/// <param name="async">Use threads</param>
		/// <param name="listenAsync">Use threads to listen for connections</param>
		/// <param name="timeout">Timeout in seconds (default no timeout)</param>
		public TCPSocketServer(IPAddress listenAddress, int port, bool async, bool listenAsync, int timeout = -1)
		: base(listenAddress, port, async)
		{
			this.timeout = timeout;
			this.listenAsync = listenAsync;
		}

		public override void Dispose()
		{
			threadAlive = false;
			lock (this)
			{
				base.Dispose();
				isListening = false;

				if (tcpListenSocket != null)
				{
					try
					{
						tcpListenSocket.Close();
					} catch { }

					try
					{
						tcpListenSocket.Dispose();
					} catch { }

					tcpListenSocket = null;
				}
			}

			ListenDisconnectedErrorCallback = null;
		}

		public bool IsListening()
		{
			lock (this) return !isDisposed && isListening;
		}

		/// <summary>
		/// Start listening for connections
		/// </summary>
		/// <param name="maxConnections">Max number of allowed connections</param>
		public void Listen(int maxConnections)
		{
			lock (this)
			{
				if (isDisposed || tcpListenSocket != null) throw new Exception("Can only be called once");
				tcpListenSocket = new NativeSocket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				tcpListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endPoint to be reused if app fails to close it on app quit
                tcpListenSocket.Bind(new IPEndPoint(address, port));
				tcpListenSocket.Listen(maxConnections);
				isListening = true;

				if (listenAsync)
				{
					thread = new Thread(AsyncThread);
					thread.IsBackground = true;
					threadAlive = true;
					thread.Start();
				}
			}
		}

		/// <summary>
		/// Accepts a socket connection
		/// </summary>
		/// <returns>Connection</returns>
		public TCPSocketConnection AcceptSync(bool blocking)
		{
            if (listenAsync) throw new Exception("Can only be called with listenAsync mode off");

			// do not-blocking check
			if (!blocking)
			{
				if (!tcpListenSocket.Poll(0, SelectMode.SelectRead)) return null;
			}

			// connect socket
            return AcceptSync_Internal(true, true);
        }

		private TCPSocketConnection AcceptSync_Internal(bool blocking, bool invokeCallback)
		{
			// do not-blocking check
			if (!blocking)
			{
				if (!tcpListenSocket.Poll(0, SelectMode.SelectRead)) return null;
			}

			// connect socket
            var socket = tcpListenSocket.Accept();
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endPoint to be reused if app fails to close it on app quit
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
            if (timeout >= 0) socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout * 1000);

            var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            var localEndPoint = socket.LocalEndPoint as IPEndPoint;
            if (remoteEndPoint == null) remoteEndPoint = new IPEndPoint(IPAddress.None, port);
            if (localEndPoint == null) localEndPoint = new IPEndPoint(IPAddress.None, port);
            lock (this)
            {
                if (isDisposed) return null;
                var connection = new TCPSocketConnection(this, socket, remoteEndPoint.Address, port, localEndPoint.Address, async);
                _connections.Add(connection);
                connection.Init();
				if (invokeCallback) InvokeConnectedCallback(this, connection, true, null);
				return connection;
            }
        }

		private void AsyncThread(object obj)
		{
			int failedConnectionCount = 0;
			while (threadAlive && !isDisposed)
			{
				TCPSocketConnection connection = null;

				try
				{
					connection = AcceptSync_Internal(true, false);
                    failedConnectionCount = 0;// reset if good connection made
                }
				catch (Exception e)
				{
					failedConnectionCount++;

					if (connection != null) connection.Dispose();
					InvokeConnectedCallback(this, connection, false, e.Message);

					if (failedConnectionCount == 5)// if we keep failing to make connections, stop listening
					{
						try
						{
							ListenDisconnectedErrorCallback?.Invoke(this, e.Message);
						}
						catch (Exception e2)
						{
							InvokeGeneralErrorCallback(this, e2);
						}
						break;
					}

					continue;
				}

				InvokeConnectedCallback(this, connection, true, null);
			}

			isListening = false;
			threadAlive = false;
		}
	}
}
