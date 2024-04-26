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
		ConnectionRequest,
		ConnectionResponse_Success,
		ConnectionResponse_Rejected,

		Send,
		SendResponse
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RUPDPacketHeader
	{
		public RUDPPacketType type;
		public uint id;
		public Guid addressID;
		public int port;
		public int dataSize;

		public RUPDPacketHeader(uint id, Guid addressID, int port, int dataSize, RUDPPacketType type)
		{
			this.id = id;
			this.addressID = addressID;
			this.port = port;
			this.dataSize = dataSize;
			this.type = type;
		}
	}

	class RUDPBufferPool
	{
		public class Pool
		{
			public bool inUse;
			public byte[] data;
			public int usedDataSize;

			public Pool(int size)
			{
				data = new byte[size];
			}
		}

		private List<Pool> pools = new List<Pool>();

		public Pool GetAvaliable(int size)
		{
			Pool pool = null;

			lock (this)
			{
				// try and find an existing avaliable pool
				foreach (var p in pools)
				{
					if (!p.inUse)
					{
						pool = p;
						break;
					}
				}

				// if no avaliable pool found, create one
				if (pool == null)
				{
					pool = new Pool(size);
					pools.Add(pool);
				}

				// if pool size is smaller than request size, increase it
				if (pool.data.Length < size) Array.Resize<byte>(ref pool.data, size);

				// mark pool as being used & its set its used size
				pool.usedDataSize = size;
				pool.inUse = true;
			}

			return pool;
		}
	}

	public class RUDPSocket : Socket
	{
		public UDPSocket udpSocket { get; private set; }// TODO: this should be singular socket for all traffic and connections just use this with their remote address
		public List<RUDPSocketConnection> connections { get; private set; }

		private bool listenCalled;
		private Guid connectionValidationID;
		private Timer tryToConnectTimer;

		private readonly IPAddress localAddress;
		private readonly int port;
		private readonly int receiveBufferSize;
		private readonly bool async;

		internal RUDPBufferPool bufferPool;
		private uint nextConnectingPacketID;
		private Dictionary<uint, RUDPBufferPool.Pool> connectingBuffers;

		public delegate void ListenDisconnectedErrorCallbackMethod(RUDPSocket sender, string message);
		public event ListenDisconnectedErrorCallbackMethod ListenDisconnectedErrorCallback;

		public delegate void ConnectedCallbackMethod(RUDPSocket sender, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;

		public RUDPSocket(IPAddress localAddress, int port, int receiveBufferSize, bool async = true)
		: base(localAddress, port)
		{
			this.localAddress = localAddress;
			this.port = port;
			this.receiveBufferSize = receiveBufferSize;
			this.async = async;

			bufferPool = new RUDPBufferPool();
			connectingBuffers = new Dictionary<uint, RUDPBufferPool.Pool>();
		}

		public override void Dispose()
		{
			isDisposed = true;
			lock (this)
			{
				ListenDisconnectedErrorCallback = null;
				ConnectedCallback = null;

				if (udpSocket != null)
				{
					udpSocket.DataRecievedCallback -= Socket_DataRecievedCallback;
					udpSocket.DisconnectedCallback -= Socket_DisconnectedCallback;
					udpSocket.Dispose();
					udpSocket = null;
				}
				connections = null;
				connectingBuffers = null;
			}
			base.Dispose();
		}

		public void Listen(int maxConnections)
		{
			if (listenCalled) throw new Exception("Listen already called");
			listenCalled = true;

			connectionValidationID = Guid.NewGuid();
			connections = new List<RUDPSocketConnection>(maxConnections);
			udpSocket = new UDPSocket(IPAddress.Any, localAddress, port, false, receiveBufferSize, async:async);
			udpSocket.DataRecievedCallback += Socket_DataRecievedCallback;
			udpSocket.DisconnectedCallback += Socket_DisconnectedCallback;
		}

		public void Connect(IPAddress remoteAddress)
		{
			if (!listenCalled) throw new Exception("Listen must be called first");
			lock (this)
			{
				connectingBuffers.Add(nextConnectingPacketID, null);

				nextConnectingPacketID++;
				if (nextConnectingPacketID == uint.MaxValue) nextConnectingPacketID = 0;// max-value is reserved for connection tests
			}
		}

		private unsafe void Socket_DataRecievedCallback(UDPSocket socket, byte[] data, int size)
		{
			int headerSize = Marshal.SizeOf<RUPDPacketHeader>();
			if (size < headerSize) return;// make sure packet is at least the size of the header

			int dataRead = 0;
			fixed (byte* dataPtr = data)
			{
				// guarantee all packets are processed (multiple can come in at once)
				byte* dataPtrOffset = dataPtr;
				while (dataRead < size)
				{
					// read header
					var header = (RUPDPacketHeader*)dataPtrOffset;
					dataRead += headerSize;

					// validate expected data size
					if (size < headerSize + header->dataSize) return;

					// process packet
					if (header->type == RUDPPacketType.ConnectionRequest && header->id == uint.MaxValue)
					{
						bool isValidRequest = false;
						lock (this)
						{
							if (isDisposed) return;

							// check if connection already exists
							bool connectionExist = false;
							foreach (var connection in connections)
							{
								if (connection.addressID == header->addressID)
								{
									connectionExist = true;
									break;
								}
							}

							// check if request is valid
							isValidRequest = header->addressID != Guid.Empty && header->port > 0;

							// add connection
							if (!connectionExist && isValidRequest)
							{
								var remoteAddress = new IPAddress(header->addressID.ToByteArray());
								var connection = new RUDPSocketConnection(this, remoteAddress, header->addressID, header->port);
								connections.Add(connection);
							}
						}

						// respond connection request was recieved
						if (isDisposed) return;
						try
						{
							header->type = isValidRequest ? RUDPPacketType.ConnectionResponse_Success : RUDPPacketType.ConnectionResponse_Rejected;
							socket.Send(data, 0, size);
						}
						catch { }
					}
					else if (header->type == RUDPPacketType.ConnectionResponse_Success && header->id == uint.MaxValue)
					{
						lock (this)
						{
							if (isDisposed) return;

							// check if connection already exists
							bool connectionExist = false;
							foreach (var connection in connections)
							{
								if (connection.addressID == header->addressID)
								{
									connectionExist = true;
									break;
								}
							}

							// add connection
							if (!connectionExist)
							{
								var remoteAddress = new IPAddress(header->addressID.ToByteArray());
								var connection = new RUDPSocketConnection(this, remoteAddress, header->addressID, header->port);
								connections.Add(connection);
							}

							// remove connecting buffer
							if (connectingBuffers.ContainsKey(header->id))
							{
								connectingBuffers[header->id].inUse = false;
								connectingBuffers.Remove(header->id);
							}
						}
					}
					else if (header->type == RUDPPacketType.ConnectionResponse_Rejected && header->id == uint.MaxValue)
					{
						lock (this)
						{
							if (isDisposed) return;

							// remove connecting buffer
							if (connectingBuffers.ContainsKey(header->id))
							{
								connectingBuffers[header->id].inUse = false;
								connectingBuffers.Remove(header->id);
							}
						}

						var address = new IPAddress(header->addressID.ToByteArray());
						ConnectedCallback?.Invoke(this, false, "Failed to connect for: " + address.ToString());
					}
					else if (header->type == RUDPPacketType.Send)
					{
						lock (this)
						{
							if (isDisposed) return;
							foreach (var connection in connections)
							{
								if (connection.addressID == header->addressID && connection.port == header->port)
								{
									connection.FireDataRecievedCallback(data, dataRead, header->dataSize);
									break;
								}
							}
						}
					}
					else if (header->type == RUDPPacketType.SendResponse)
					{
						lock (this)
						{
							if (isDisposed) return;
							foreach (var connection in connections)
							{
								if (connection.addressID == header->addressID && connection.port == header->port)
								{
									connection.DataSentResponse(header->id);
									break;
								}
							}
						}
					}

					// offset read data
					dataRead += header->dataSize;
					dataPtrOffset += headerSize + header->dataSize;// offset data pointer
				}
			}
		}

		private void Socket_DisconnectedCallback(UDPSocket socket)
		{
			ListenDisconnectedErrorCallback?.Invoke(this, null);
		}

		public override bool IsConnected()
		{
			lock (this) return !isDisposed && connections != null && connections.Count != 0;
		}
	}
}