using System.Collections.Generic;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Orbital.Networking.Sockets
{
	enum RUDPPacketType : byte
	{
		Send,
		Recieved
	}

	[StructLayout(LayoutKind.Sequential)]
	struct RUPDPacketHeader
	{
		public uint id;
		public uint dataSize;
		public RUDPPacketType type;

		public RUPDPacketHeader(uint id, uint dataSize, RUDPPacketType type)
		{
			this.id = id;
			this.dataSize = dataSize;
			this.type = type;
		}
	}

	public class RUDPSocketConnection : Socket, INetworkDataSender, IDisposable
	{
		class BufferPool
		{
			public const int defaultDataSize = 256;

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
				// try and find an existing avaliable pool
				Pool pool = null;
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

				return pool;
			}
		}

		public UDPSocket socket { get; private set; }
		private Guid connectionTestID;
		private bool isConnected;
		private Timer tryToConnectTimer;
		private BufferPool bufferPool;
		private uint nextPacketID;
		private Dictionary<uint, BufferPool.Pool> sendingBuffers;

		public delegate void ConnectedCallbackMethod(RUDPSocketConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;

		public delegate void DataRecievedCallbackMethod(RUDPSocketConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public RUDPSocketConnection(IPAddress remoteAddress, IPAddress localAddress, int port, int receiveBufferSize, bool async)
		: base(remoteAddress, port)
		{
			sendingBuffers = new Dictionary<uint, BufferPool.Pool>();
			bufferPool = new BufferPool();

			socket = new UDPSocket(remoteAddress, localAddress, port, false, receiveBufferSize, async:async);
			socket.DataRecievedCallback += Socket_DataRecievedCallback;
			socket.DisconnectedCallback += Socket_DisconnectedCallback;
		}

		public override void Dispose()
		{
			if (socket != null)
			{
				socket.DataRecievedCallback -= Socket_DataRecievedCallback;
				socket.DisconnectedCallback -= Socket_DisconnectedCallback;
				socket.Dispose();
				socket = null;
			}
		}

		/// <summary>
		/// Start attempting a connection
		/// </summary>
		public void Connect()
		{
			socket.Join(true);
			tryToConnectTimer = new Timer(TryToConnectCallback, null, 0, 500);
		}

		private void TryToConnectCallback(object state)
		{
			if (!isConnected)
			{
				unsafe
				{
					fixed (Guid* id = &connectionTestID)
					{
						SendInternalPacket(uint.MaxValue, (byte*)id, Marshal.SizeOf<Guid>(), RUDPPacketType.Send);
					}
				}
			}
		}

		private unsafe void Socket_DataRecievedCallback(UDPSocket socket, byte[] data, int size)
		{
			int headerSize = Marshal.SizeOf<RUPDPacketHeader>();
			int dataRead = 0;
			fixed (byte* dataPtr = data)
			{
				// guarantee all packets are processed
				byte* dataPtrOffset = dataPtr;
				while (dataRead < size)
				{
					// make sure packet is at least the size of the header
					if (size < headerSize) return;

					// read header
					var header = (RUPDPacketHeader*)dataPtrOffset;
					dataRead += Marshal.SizeOf<RUPDPacketHeader>();

					// validate expected data size
					if (size < headerSize + header->dataSize) return;

					// check if this is a connection validation communication
					if (header->id == uint.MaxValue && size == headerSize + Marshal.SizeOf<Guid>())
					{
						if (header->type == RUDPPacketType.Recieved)// check if this is a connection validation response
						{
							var connectionID = (Guid*)(dataPtrOffset + headerSize);
							if (*connectionID == connectionTestID && !isConnected)
							{
								if (tryToConnectTimer != null)
								{
									tryToConnectTimer.Dispose();
									tryToConnectTimer = null;
								}
								isConnected = true;
								ConnectedCallback?.Invoke(this, true, null);
							}
						}
						else// check if this is a connection validation request
						{
							header->type = RUDPPacketType.Recieved;
							socket.Send(data, 0, size);
						}
					}

					// only read packet data if connected
					else if (isConnected)
					{
						if (header->type == RUDPPacketType.Send)
						{
							DataRecievedCallback?.Invoke(this, data, size);
							SendInternalPacket(header->id, null, 0, RUDPPacketType.Recieved);// respond packet recieved to sender
						}
						else if (header->type == RUDPPacketType.Recieved)
						{
							// remove recieved packets
							if (sendingBuffers.ContainsKey(header->id))
							{
								sendingBuffers[header->id].inUse = false;
								sendingBuffers.Remove(header->id);
							}
						}
					}

					// offset read data
					dataRead += headerSize + (int)header->dataSize;
					dataPtrOffset += headerSize + header->dataSize;// offset data pointer
				}
			}
		}

		private void Socket_DisconnectedCallback(UDPSocket socket)
		{
			isConnected = false;
		}

		public override bool IsConnected()
		{
			return isConnected;
		}

		private unsafe void SendInternalPacket(uint id, byte* data, int dataSize, RUDPPacketType type)
		{
			int headerSize = Marshal.SizeOf<RUPDPacketHeader>();

			// get avaliable pool
			var pool = bufferPool.GetAvaliable(headerSize + dataSize);

			// copy header & data into packet-data
			var header = new RUPDPacketHeader(id, (uint)dataSize, type);
			fixed (byte* packetDataPtr = pool.data)
			{
				Buffer.MemoryCopy(&header, packetDataPtr, headerSize, headerSize);
				if (data != null) Buffer.MemoryCopy(data, packetDataPtr + headerSize, dataSize, dataSize);
			}

			// send packet
			socket.Send(pool.data, 0, pool.usedDataSize);

			// make pool as no longer used
			pool.inUse = false;
		}

		private unsafe int SendPacket(byte* buffer, int offset, int size)
		{
			int headerSize = Marshal.SizeOf<RUPDPacketHeader>();

			// get avaliable pool
			var pool = bufferPool.GetAvaliable(headerSize + size);
			sendingBuffers.Add(nextPacketID, pool);

			// copy header & data into pool
			unsafe
			{
				var header = new RUPDPacketHeader(nextPacketID, (uint)size, RUDPPacketType.Send);
				fixed (byte* poolDataPtr = pool.data)
				{
					Buffer.MemoryCopy(&header, poolDataPtr, headerSize, headerSize);
					Buffer.MemoryCopy(buffer, poolDataPtr + headerSize, size, size);
				}
			}

			// send packet
			int bytesSent = socket.Send(pool.data, offset, size);

			// increment packet
			++nextPacketID;
			if (nextPacketID == uint.MaxValue) nextPacketID = 0;// max-value is reserved for connection tests

			// return how many bytes sent
			return bytesSent;
		}

		public unsafe int Send(byte* buffer, int length)
		{
			lock (this)
			{
				return SendPacket(buffer, 0, length);
			}
		}

		public unsafe int Send(byte* buffer, int offset, int length)
		{
			lock (this)
			{
				return SendPacket(buffer, offset, length);
			}
		}

		public unsafe int Send(byte[] buffer)
		{
			lock (this)
			{
				fixed (byte* bufferPtr = buffer) return SendPacket(bufferPtr, 0, buffer.Length);
			}
		}

		public unsafe int Send(byte[] buffer, int offset, int size)
		{
			lock (this)
			{
				fixed (byte* bufferPtr = buffer) return SendPacket(bufferPtr, offset, size);
			}
		}

		public int Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			return Send(data);
		}

		public unsafe int Send<T>(T* data) where T : struct
		{
			return Send((byte*)data, Marshal.SizeOf<T>());
		}
	}
}
