﻿using System;
using System.Net;
using System.Net.Sockets;

using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
    public class TCPSocketServer : TCPSocket
    {
		private bool isListening;
		private NativeSocket tcpListenSocket;
		private int sendTimeout, receiveTimeout;

		public delegate void ListenDisconnectedErrorCallbackMethod(TCPSocketServer sender, string message);
		public event ListenDisconnectedErrorCallbackMethod ListenDisconnectedErrorCallback;

		public TCPSocketServer(IPAddress listenAddress, int port, int sendTimeout = -1, int receiveTimeout = -1, bool async = true)
		: base(listenAddress, port, async)
		{
			this.sendTimeout = sendTimeout;
			this.receiveTimeout = receiveTimeout;
			if (sendTimeout >= 0 || receiveTimeout >= 0)
			{
				if (sendTimeout < 0 || receiveTimeout < 0) throw new ArgumentException("'sendTimeout' and 'receiveTimeout' must both be >= 0 if either is");
			}
		}

		public override void Dispose()
		{
			isDisposed = true;

			lock (this)
			{
				isListening = false;

				if (tcpListenSocket != null)
				{
					try
					{
						tcpListenSocket.Shutdown(SocketShutdown.Both);
					}
					catch { }

					tcpListenSocket.Close();
					tcpListenSocket.Dispose();
					tcpListenSocket = null;
				}
			}

			base.Dispose();
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
				tcpListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endpoint to be reused if Windows fails to close it on app quit
				tcpListenSocket.Bind(new IPEndPoint(address, port));
				tcpListenSocket.Listen(maxConnections);
				isListening = true;
				try
				{
					tcpListenSocket.BeginAccept(ConnectionCallback, null);
				}
				catch (Exception e)
				{
					isListening = false;
					throw e;
				}
			}
		}

		private void ConnectionCallback(IAsyncResult ar)
		{
			NativeSocket socket = null;
			TCPSocketConnection connection = null;
			bool success = false;
			string message = null;

			// connect
			lock (this)
			{
				if (isDisposed) return;
				
				try
				{
					socket = tcpListenSocket.EndAccept(ar);
					if (sendTimeout >= 0 || receiveTimeout >= 0) socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
					if (sendTimeout >= 0) socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, sendTimeout);
					if (receiveTimeout >= 0) socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, receiveTimeout);
					var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
					var localEndPoint = socket.LocalEndPoint as IPEndPoint;
					if (remoteEndPoint == null) remoteEndPoint = new IPEndPoint(IPAddress.None, port);
					if (localEndPoint == null) localEndPoint = new IPEndPoint(IPAddress.None, port);
					connection = new TCPSocketConnection(this, socket, remoteEndPoint.Address, port, localEndPoint.Address, async);
					_connections.Add(connection);
					success = true;
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
						connection.Dispose();
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

				if (disconnected) connection.Dispose();
			}

			// listen for next connection
			string listenError = null;
			lock (this)
			{
				if (!isDisposed)
				{
					try
					{
						tcpListenSocket.BeginAccept(ConnectionCallback, address);
					}
					catch (Exception e)
					{
						isListening = false;
						listenError = e.Message;
					}
				}
			}

			if (listenError != null) ListenDisconnectedErrorCallback?.Invoke(this, listenError);
		}
	}
}
