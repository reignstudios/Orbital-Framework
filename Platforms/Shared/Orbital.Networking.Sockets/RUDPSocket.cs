using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Orbital.Networking.Sockets
{
	enum RUDPPacketType : byte
	{
		Send,
		SendWasRecieved
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RUPDPacketHeader
	{
		public uint id;
		public int dataSize;
		public RUDPPacketType type;

		public RUPDPacketHeader(uint id, int dataSize, RUDPPacketType type)
		{
			this.id = id;
			this.dataSize = dataSize;
			this.type = type;
		}
	}

	public class RUDPSocket : IDisposable
	{
		public UDPSocket listenSocket { get; private set; }// TODO: this should be singular socket for all traffic and connections just use this with their remote address
		public List<RUDPSocketConnection> connections { get; private set; }

		private bool listenCalled;
		private Guid connectionValidationID;
		private Timer tryToConnectTimer;

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
			if (listenCalled) throw new Exception("Listen already called");
			listenCalled = true;

			connectionValidationID = Guid.NewGuid();
			connections = new List<RUDPSocketConnection>(maxConnections);
			listenSocket = new UDPSocket(IPAddress.Any, listenAddress, port, false, receiveBufferSize, async:async);
			listenSocket.DataRecievedCallback += Socket_DataRecievedCallback;
			listenSocket.DisconnectedCallback += Socket_DisconnectedCallback;
		}

		public void Connect(IPAddress remoteAddress)
		{
			if (!listenCalled) throw new Exception("Listen must be called first");
			// TODO
		}

		private unsafe void Socket_DataRecievedCallback(UDPSocket socket, byte[] data, int size)
		{
			int headerSize = Marshal.SizeOf<RUPDPacketHeader>();
			if (size < headerSize) return;// make sure packet is at least the size of the header

			int dataRead = 0;
			fixed (byte* dataPtr = data)
			{
				// guarantee all packets are processed
				byte* dataPtrOffset = dataPtr;
				while (dataRead < size)
				{
					// read header
					var header = (RUPDPacketHeader*)dataPtrOffset;
					dataRead += headerSize;

					// validate expected data size
					if (size < headerSize + header->dataSize) return;

					// check if this is a connection validation communication
					if (header->id == uint.MaxValue)
					{
						if (header->type == RUDPPacketType.Send)// check if this is a connection validation response
						{
							var connectionID = (Guid*)(dataPtrOffset + headerSize);
							if (*connectionID == connectionValidationID)
							{
								header->type = RUDPPacketType.SendWasRecieved;
								socket.Send(data, 0, size);
								// TODO: add connection if it doesn't exist (this packet needs to contain remote address)
							}
						}
					}

					// offset read data
					dataRead += headerSize + header->dataSize;
					dataPtrOffset += headerSize + header->dataSize;// offset data pointer
				}
			}
		}

		private void Socket_DisconnectedCallback(UDPSocket socket)
		{
			ListenDisconnectedErrorCallback?.Invoke(this, null);
		}
	}
}