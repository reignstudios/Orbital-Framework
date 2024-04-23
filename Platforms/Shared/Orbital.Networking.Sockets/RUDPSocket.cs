using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Orbital.Networking.Sockets
{
	public class RUDPSocket : IDisposable
	{
		public UDPSocket listenSocket { get; private set; }
		public List<RUDPSocketConnection> connections { get; private set; }

		private readonly IPAddress listenAddress;
		private readonly int port;
		private readonly int receiveBufferSize;
		private readonly bool async;

		public delegate void ListenDisconnectedErrorCallbackMethod(RUDPSocket sender, string message);
		public event ListenDisconnectedErrorCallbackMethod ListenDisconnectedErrorCallback;

		public RUDPSocket(IPAddress listenAddress, int port, int receiveBufferSize, bool async = true)
		{
			this.listenAddress = listenAddress;
			this.port = port;
			this.receiveBufferSize = receiveBufferSize;
			this.async = async;
		}

		public void Dispose()
		{
			ListenDisconnectedErrorCallback = null;
			if (listenSocket != null)
			{
				listenSocket.DataRecievedCallback -= Socket_DataRecievedCallback;
				listenSocket.DisconnectedCallback -= Socket_DisconnectedCallback;
				listenSocket.Dispose();
				listenSocket = null;
			}
			connections = null;
		}

		public void Listen(int maxConnections)
		{
			connections = new List<RUDPSocketConnection>(maxConnections);
			listenSocket = new UDPSocket(IPAddress.Any, listenAddress, port, false, receiveBufferSize, async:async);
			listenSocket.DataRecievedCallback += Socket_DataRecievedCallback;
			listenSocket.DisconnectedCallback += Socket_DisconnectedCallback;
		}

		private unsafe void Socket_DataRecievedCallback(UDPSocket socket, byte[] data, int size)
		{

		}

		private void Socket_DisconnectedCallback(UDPSocket socket)
		{
			ListenDisconnectedErrorCallback?.Invoke(this, null);
		}
	}
}