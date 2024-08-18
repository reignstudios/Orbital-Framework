using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Orbital.Networking.Sockets
{
	enum RUDPPacketType : byte
	{
		UnreliableData,

		ConnectionRequest,
		ConnectionResponse_Success,
		ConnectionResponse_Rejected,

		Send,
		SendResponse
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RUDPPacketHeader_Unreliable
	{
		public RUDPPacketType type;
		public int packetSize;

		public RUDPPacketHeader_Unreliable(int packetSize)
		{
			type = RUDPPacketType.UnreliableData;
			this.packetSize = packetSize;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RUDPPacketHeader
	{
		public RUDPPacketType type;
		public uint id;
		public Guid senderAddressID, targetAddressID;
		public int port;
		public int dataSize;

		public RUDPPacketHeader(uint id, Guid senderAddressID, Guid targetAddressID, int port, int dataSize, RUDPPacketType type)
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

	public class RUDPSocket : Socket, INetworkDataSender
	{
		public UDPSocket udpSocket { get; private set; }

		private List<RUDPSocketConnection> _connections;
		public IReadOnlyList<RUDPSocketConnection> connections => _connections;

		private bool listenCalled;
		private Timer tryToConnectTimer;

		public IPAddress listenAddress => address;
		public Guid listenAddressID => addressID;
		public readonly IPAddress senderAddress;
		public readonly Guid senderAddressID;
		public readonly int port;
		public readonly int maxBufferSize;
		internal readonly int timeout;
		internal readonly bool useBurst;
		internal readonly int burstCount;
		private const int burstConnectionCount = 5;

		internal RUDPBufferPool bufferPool;
		private Dictionary<uint, RUDPBufferPool.Pool> connectingBuffers;
		private uint nextConnectingPacketID;

		public delegate void ListenDisconnectedErrorCallbackMethod(RUDPSocket socket, string message);
		public event ListenDisconnectedErrorCallbackMethod ListenDisconnectedErrorCallback;

		public delegate void ConnectedCallbackMethod(RUDPSocket socket, RUDPSocketConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;

		public delegate void UnreliableDataRecievedCallbackMethod(byte[] data, int offset, int size);
		public event UnreliableDataRecievedCallbackMethod UnreliableDataRecievedCallback;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="listenAddress">Address to listen for connection requests on</param>
		/// <param name="senderAddress">Address we are sending from (this must be a public address a reciever can responsd to)</param>
		/// <param name="port">Port all traffic is sent over</param>
		/// <param name="maxBufferSize">Max size a package can be</param>
		/// <param name="timeout">Timeout in seconds (default no timeout)</param>
		public RUDPSocket(IPAddress listenAddress, IPAddress senderAddress, int port, int maxBufferSize, int timeout = -1, bool useBurst = true, int burstCount = 3)
		: base(listenAddress, port)
		{
			this.senderAddress = senderAddress;
			this.senderAddressID = AddressToAddressID(senderAddress);
			this.port = port;
			this.maxBufferSize = Math.Max(maxBufferSize, Marshal.SizeOf<RUDPPacketHeader>());
			this.timeout = timeout;
			this.useBurst = useBurst;
			this.burstCount = burstCount;

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

		public void Listen(int maxConnections)
		{
			if (listenCalled) throw new Exception("Listen already called");
			listenCalled = true;

			_connections = new List<RUDPSocketConnection>(maxConnections);
			udpSocket = new UDPSocket(IPAddress.Any, listenAddress, port, false, maxBufferSize);
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
				int headerSize = Marshal.SizeOf<RUDPPacketHeader>();
				var pool = bufferPool.GetAvaliable(headerSize);
				
				// copy header & data into packet-data
				var remoteAddressID = AddressToAddressID(remoteAddress);
				var header = new RUDPPacketHeader(nextConnectingPacketID, senderAddressID, remoteAddressID, port, 0, RUDPPacketType.ConnectionRequest);
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
							var header = (RUDPPacketHeader*)dataPtr;

							// create endpoint
							var remoteAddress = AddressIDToAddress(header->targetAddressID);
							var remoteEndPoint = new IPEndPoint(remoteAddress, header->port);

							// send connection request
							try
							{
								for (int i = 0; i < burstConnectionCount; ++i)
								{
									udpSocket.Send(pool.data, 0, pool.usedDataSize, remoteEndPoint);
								}
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
			if (size <= 0) return;

			int headerSize = Marshal.SizeOf<RUDPPacketHeader>();
			int dataRead = 0;
			fixed (byte* dataPtr = data)
			{
				// guarantee all packets are processed (multiple can come in at once)
				byte* dataPtrOffset = dataPtr;
				while (dataRead < size)
				{
					// process unreliable packet
					var packetType = (RUDPPacketType)data[dataRead];
					if (packetType == RUDPPacketType.UnreliableData)
					{
						// read header
						int headSizeUnreliable = sizeof(RUDPPacketHeader_Unreliable);
						if (size < headSizeUnreliable) return;// make sure packet is at least the size of the header
						var headerUnreliable = (RUDPPacketHeader_Unreliable*)dataPtrOffset;
						dataRead += headSizeUnreliable;
						dataPtrOffset += headSizeUnreliable;

						// invoke data recieved
						try
						{
							UnreliableDataRecievedCallback?.Invoke(data, dataRead, headerUnreliable->packetSize);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							System.Diagnostics.Debug.WriteLine(e);
						}

						// finish
						dataRead += headerUnreliable->packetSize;
						dataPtrOffset += headerUnreliable->packetSize;
						continue;
					}

					// read header
					if (size < headerSize) return;// make sure packet is at least the size of the header

					var header = (RUDPPacketHeader*)dataPtrOffset;
					dataRead += headerSize;

					// validate expected data size
					if (size < headerSize + header->dataSize) goto CONTINUE_READ;

					// validate target
					if (header->targetAddressID != senderAddressID || header->port != port) goto CONTINUE_READ;

					// process reliable packet
					if (header->type == RUDPPacketType.ConnectionRequest)
					{
						bool isValidRequest = false;
						RUDPSocketConnection madeConnection = null, existingConnection = null;
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
									existingConnection = connection;
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
							for (int i = 0; i < burstConnectionCount; ++i)
							{
								if (madeConnection != null) socket.Send(data, dataRead - headerSize, headerSize + header->dataSize, madeConnection.endPoint);
								else if (existingConnection != null) socket.Send(data, dataRead - headerSize, headerSize + header->dataSize, existingConnection.endPoint);
								else throw new Exception("No response connection request found");
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							System.Diagnostics.Debug.WriteLine(e);
						}

						// fire connection made callback
						if (madeConnection != null && ConnectedCallback != null)
						{
							try
							{
								ConnectedCallback(this, madeConnection, true, null);
							}
							catch (Exception e)
							{
								Console.WriteLine(e);
								System.Diagnostics.Debug.WriteLine(e);
							}
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
							try
							{
								ConnectedCallback(this, madeConnection, true, null);
							}
							catch (Exception e)
							{
								Console.WriteLine(e);
								System.Diagnostics.Debug.WriteLine(e);
							}
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
						try
						{
							ConnectedCallback?.Invoke(this, null, false, "Failed to connect for: " + address.ToString());
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							System.Diagnostics.Debug.WriteLine(e);
						}
					}
					else if (header->type == RUDPPacketType.Send)
					{
						RUDPSocketConnection sendingConnection = null;
						lock (this)
						{
							if (isDisposed) return;
							foreach (var connection in _connections)
							{
								if (connection.addressID == header->senderAddressID)
								{
									sendingConnection = connection;
									connection.FireDataRecievedCallback(header, data, dataRead, header->dataSize);
									break;
								}
							}
						}

						// respond data was recieved
						try
						{
							header->type = RUDPPacketType.SendResponse;
							header->targetAddressID = header->senderAddressID;// target is now sender
							header->senderAddressID = senderAddressID;// sender is now us
							for (int i = 0; i < burstConnectionCount; ++i)
							{
								socket.Send(data, dataRead - headerSize, headerSize + header->dataSize, sendingConnection.endPoint);
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							System.Diagnostics.Debug.WriteLine(e);
						}
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

		private void Socket_DisconnectedCallback(UDPSocket socket, string message)
		{
			lock (this)
			{
				if (_connections != null)
				{
					foreach (var connection in _connections)
					{
						connection.Dispose(message);
					}
					_connections = null;
				}
			}

			try
			{
				ListenDisconnectedErrorCallback?.Invoke(this, message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				System.Diagnostics.Debug.WriteLine(e);
			}
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

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send(byte* data, int size)
		{
			udpSocket.Send(data, size);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send(byte* data, int size, EndPoint endpoint)
		{
			udpSocket.Send(data, size, endpoint);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send(byte* data, int offset, int size)
		{
			udpSocket.Send(data, offset, size);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send(byte* data, int offset, int size, EndPoint endpoint)
		{
			udpSocket.Send(data, offset, size, endpoint);
		}
		
		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send<T>(T data) where T : unmanaged
		{
			udpSocket.Send<T>(&data);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send<T>(T data, EndPoint endpoint) where T : unmanaged
		{
			udpSocket.Send<T>(&data, endpoint);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send<T>(T* data) where T : unmanaged
		{
			udpSocket.Send<T>(data);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public unsafe void Send<T>(T* data, EndPoint endpoint) where T : unmanaged
		{
			udpSocket.Send<T>(data, endpoint);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(byte[] data)
		{
			udpSocket.Send(data);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(byte[] data, EndPoint endpoint)
		{
			udpSocket.Send(data, endpoint);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(byte[] data, int size)
		{
			udpSocket.Send(data, size);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(byte[] data, int size, EndPoint endpoint)
		{
			udpSocket.Send(data, size, endpoint);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(byte[] data, int offset, int size)
		{
			udpSocket.Send(data, offset, size);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(byte[] data, int offset, int size, EndPoint endpoint)
		{
			udpSocket.Send(data, offset, size, endpoint);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(string text, Encoding encoding)
		{
			udpSocket.Send(text, encoding);
		}

		/// <summary>
		/// Send 'unreliable' data (use RUDPSocketConnection to send reliable data)
		/// NOTE: This data must be prefixed with 'RUDPPacketHeader_Unreliable' header
		/// </summary>
		public void Send(string text, Encoding encoding, EndPoint endpoint)
		{
			udpSocket.Send(text, encoding, endpoint);
		}
	}
}