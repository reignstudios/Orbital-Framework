using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
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
		public Guid senderAddressID, targetAddressID;
		public int port;
		public int dataSize;

		public RUPDPacketHeader(uint id, Guid senderAddressID, Guid targetAddressID, int port, int dataSize, RUDPPacketType type)
		{
			this.id = id;
			this.senderAddressID = senderAddressID;
			this.targetAddressID = targetAddressID;
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
			public DateTime usedAtTime;

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
				pool.usedAtTime = DateTime.Now;
			}

			return pool;
		}
	}

	public class RUDPSocket : Socket
	{
		public UDPSocket udpSocket { get; private set; }

		private List<RUDPSocketConnection> _connections;
		public IReadOnlyList<RUDPSocketConnection> connections => _connections;

		private bool listenCalled;
		private Timer tryToConnectTimer;

		internal readonly IPAddress listenAddress, senderAddress;
		internal readonly Guid listenAddressID, senderAddressID;
		private readonly int port;
		private readonly int receiveBufferSize;
		private readonly bool async;

		internal RUDPBufferPool bufferPool;
		private Dictionary<uint, RUDPBufferPool.Pool> connectingBuffers;
		private uint nextConnectingPacketID;

		public delegate void ListenDisconnectedErrorCallbackMethod(RUDPSocket sender, string message);
		public event ListenDisconnectedErrorCallbackMethod ListenDisconnectedErrorCallback;

		public delegate void ConnectedCallbackMethod(RUDPSocket sender, RUDPSocketConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;

		public RUDPSocket(IPAddress listenAddress, IPAddress senderAddress, int port, int receiveBufferSize, bool async = true)
		: base(listenAddress, port)
		{
			this.listenAddress = listenAddress;
			this.senderAddress = senderAddress;
			this.listenAddressID = AddressToAddressID(listenAddress);
			this.senderAddressID = AddressToAddressID(senderAddress);
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

				if (tryToConnectTimer != null)
				{
					tryToConnectTimer.Dispose();
					tryToConnectTimer = null;
				}

				if (udpSocket != null)
				{
					udpSocket.DataRecievedCallback -= Socket_DataRecievedCallback;
					udpSocket.DisconnectedCallback -= Socket_DisconnectedCallback;
					udpSocket.Dispose();
					udpSocket = null;
				}

				if (_connections != null)
				{
					foreach (var connection in _connections)
					{
						connection.Dispose();
					}
					_connections = null;
				}

				connectingBuffers = null;
			}
			base.Dispose();
		}

		internal unsafe static IPAddress AddressIDToAddress(Guid addressID)
		{
			var bytes = addressID.ToByteArray();
			bool isIPV6 = false;
			for (int i = 4; i < 16; ++i)
			{
				if (bytes[i] != 0)
				{
					isIPV6 = true;
					break;
				}
			}

			if (!isIPV6)
			{
				var newBytes = new byte[4];
				Array.Copy(bytes, newBytes, newBytes.Length);
				bytes = newBytes;
			}

			return new IPAddress(bytes);
		}

		internal static Guid AddressToAddressID(IPAddress address)
		{
			var aadressBytes = address.GetAddressBytes();
			if (aadressBytes.Length < 16)
			{
				var newBytes = new byte[16];
				Array.Copy(aadressBytes, newBytes, aadressBytes.Length);
				aadressBytes = newBytes;
			}
			return new Guid(aadressBytes);
		}

		public void Listen(int maxConnections)
		{
			if (listenCalled) throw new Exception("Listen already called");
			listenCalled = true;

			_connections = new List<RUDPSocketConnection>(maxConnections);
			udpSocket = new UDPSocket(IPAddress.Any, listenAddress, port, false, receiveBufferSize, async:async);
			udpSocket.DataRecievedCallback += Socket_DataRecievedCallback;
			udpSocket.DisconnectedCallback += Socket_DisconnectedCallback;
			udpSocket.Join(true);
		}

		public unsafe void Connect(IPAddress remoteAddress)
		{
			if (!listenCalled) throw new Exception("Listen must be called first");
			lock (this)
			{
				// get avaliable pool
				int headerSize = Marshal.SizeOf<RUPDPacketHeader>();
				var pool = bufferPool.GetAvaliable(headerSize);
				
				// copy header & data into packet-data
				var remoteAddressID = AddressToAddressID(remoteAddress);
				var header = new RUPDPacketHeader(nextConnectingPacketID, senderAddressID, remoteAddressID, port, 0, RUDPPacketType.ConnectionRequest);
				fixed (byte* packetDataPtr = pool.data)
				{
					Buffer.MemoryCopy(&header, packetDataPtr, headerSize, headerSize);
				}

				// add to connecting buffers
				connectingBuffers.Add(nextConnectingPacketID, pool);

				// increase next packet ID
				nextConnectingPacketID++;
				if (nextConnectingPacketID == uint.MaxValue) nextConnectingPacketID = 0;
			}

			// start timer if needed
			if (tryToConnectTimer == null)
			{
				tryToConnectTimer = new Timer(ConnectWaitCallback, null, 0, 500);
			}
		}

		private unsafe void ConnectWaitCallback(object state)
		{
			lock (this)
			{
				// send connection requests or gather timeouts
				var now = DateTime.Now;
				List<uint> finishedIDs = null;
				foreach (var buffer in connectingBuffers)
				{
					var pool = buffer.Value;
					if (!pool.inUse || (now - pool.usedAtTime).TotalSeconds >= 10)
					{
						if (finishedIDs == null) finishedIDs = new List<uint>();
						finishedIDs.Add(buffer.Key);
					}
					else
					{
						fixed (byte* dataPtr = pool.data)
						{
							var header = (RUPDPacketHeader*)dataPtr;

							// create endpoint
							var remoteAddress = AddressIDToAddress(header->targetAddressID);
							var remoteEndPoint = new IPEndPoint(remoteAddress, header->port);

							// send connection request
							try
							{
								udpSocket.Send(pool.data, 0, pool.usedDataSize, remoteEndPoint);
							}
							catch { }
						}
					}
				}

				// remove timeouts
				if (finishedIDs != null)
				{
					foreach (var id in finishedIDs)
					{
						connectingBuffers.Remove(id);
					}
				}
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
					if (size < headerSize + header->dataSize) goto CONTINUE_READ;

					// validate target
					if (header->targetAddressID != senderAddressID || header->port != port) goto CONTINUE_READ;

					// process packet
					if (header->type == RUDPPacketType.ConnectionRequest)
					{
						bool isValidRequest = false;
						RUDPSocketConnection madeConnection = null;
						lock (this)
						{
							if (isDisposed) return;

							// check if connection already exists
							bool connectionExist = false;
							foreach (var connection in _connections)
							{
								if (connection.addressID == header->senderAddressID)
								{
									connectionExist = true;
									break;
								}
							}

							// check if request is valid
							isValidRequest = header->senderAddressID != Guid.Empty;

							// add connection
							if (!connectionExist && isValidRequest)
							{
								var remoteAddress = AddressIDToAddress(header->senderAddressID);
								madeConnection = new RUDPSocketConnection(this, remoteAddress, header->senderAddressID, header->port);
								_connections.Add(madeConnection);
							}
						}

						// respond connection request was recieved
						try
						{
							header->type = isValidRequest ? RUDPPacketType.ConnectionResponse_Success : RUDPPacketType.ConnectionResponse_Rejected;
							header->targetAddressID = header->senderAddressID;// target is now sender
							header->senderAddressID = senderAddressID;// sender is now us
							socket.Send(data, dataRead - headerSize, headerSize + header->dataSize);
						}
						catch { }

						// fire connection made callback
						if (madeConnection != null && ConnectedCallback != null)
						{
							ConnectedCallback(this, madeConnection, true, null);
						}
					}
					else if (header->type == RUDPPacketType.ConnectionResponse_Success)
					{
						RUDPSocketConnection madeConnection = null;
						lock (this)
						{
							if (isDisposed) return;

							// check if connection already exists
							bool connectionExist = false;
							foreach (var connection in _connections)
							{
								if (connection.addressID == header->senderAddressID)
								{
									connectionExist = true;
									break;
								}
							}

							// add connection
							if (!connectionExist)
							{
								var remoteAddress = AddressIDToAddress(header->senderAddressID);
								madeConnection = new RUDPSocketConnection(this, remoteAddress, header->senderAddressID, header->port);
								_connections.Add(madeConnection);
							}

							// remove connecting buffer
							if (connectingBuffers.ContainsKey(header->id))
							{
								connectingBuffers[header->id].inUse = false;
								connectingBuffers.Remove(header->id);
							}
						}

						// fire connection made callback
						if (madeConnection != null && ConnectedCallback != null)
						{
							ConnectedCallback(this, madeConnection, true, null);
						}
					}
					else if (header->type == RUDPPacketType.ConnectionResponse_Rejected)
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

						// fire connection failed callback
						var address = AddressIDToAddress(header->senderAddressID);
						ConnectedCallback?.Invoke(this, null, false, "Failed to connect for: " + address.ToString());
					}
					else if (header->type == RUDPPacketType.Send)
					{
						lock (this)
						{
							if (isDisposed) return;
							foreach (var connection in _connections)
							{
								if (connection.addressID == header->senderAddressID)
								{
									connection.FireDataRecievedCallback(header, data, dataRead, header->dataSize);
									break;
								}
							}
						}

						// respond connection request was recieved
						try
						{
							header->type = RUDPPacketType.SendResponse;
							header->targetAddressID = header->senderAddressID;// target is now sender
							header->senderAddressID = senderAddressID;// sender is now us
							socket.Send(data, dataRead - headerSize, headerSize + header->dataSize);
						}
						catch { }
					}
					else if (header->type == RUDPPacketType.SendResponse)
					{
						lock (this)
						{
							if (isDisposed) return;
							foreach (var connection in _connections)
							{
								if (connection.addressID == header->senderAddressID)
								{
									connection.DataSentResponse(header->id);
									break;
								}
							}
						}
					}

					// offset read data
					CONTINUE_READ:;
					dataRead += header->dataSize;
					dataPtrOffset += headerSize + header->dataSize;// offset data pointer
				}
			}
		}

		private void Socket_DisconnectedCallback(UDPSocket socket)
		{
			lock (this)
			{
				if (_connections != null)
				{
					foreach (var connection in _connections)
					{
						connection.Dispose();
					}
					_connections = null;
				}
			}

			ListenDisconnectedErrorCallback?.Invoke(this, null);
		}

		public override bool IsConnected()
		{
			lock (this) return !isDisposed && _connections != null && _connections.Count != 0;
		}

		internal void RemoveConnection(RUDPSocketConnection connection)
		{
			lock (this)
			{
				if (!isDisposed) _connections.Remove(connection);
			}
		}
	}
}