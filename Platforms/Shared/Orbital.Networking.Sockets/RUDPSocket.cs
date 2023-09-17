using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Orbital.Networking.Sockets
{
	enum RUDPPacketType : byte
	{
		Request,
		Response,
		PacketRecievedResponse
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

	public abstract class RUDPSocket : Socket, INetworkDataSender
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

		public delegate void ConnectedCallbackMethod(RUDPSocket socket, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;

		public delegate void DataRecievedCallbackMethod(RUDPSocketConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public RUDPSocket(IPAddress remoteAddress, IPAddress localAddress, int port, int receiveBufferSize)
		: base(remoteAddress, port)
		{
			sendingBuffers = new Dictionary<uint, BufferPool.Pool>();
			bufferPool = new BufferPool();

			socket = new UDPSocket(remoteAddress, localAddress, port, false, receiveBufferSize);
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
		public void Join()
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
						SendCustomPacket(uint.MaxValue, (byte*)id, Marshal.SizeOf<Guid>(), RUDPPacketType.Request);
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
						if (header->type == RUDPPacketType.Response)// check if this is a connection validation response
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
							header->type = RUDPPacketType.Response;
							socket.Send(data, 0, size);
						}
					}

					// only read packet data if connected
					else if (isConnected)
					{
						if (header->type == RUDPPacketType.Response)
						{
							if (sendingBuffers.ContainsKey(header->id))// ignore potentially redundent packets
							{
								/*if (!recievingBuffers.ContainsKey(header->id))// add new recieving buffer
								{
									//var parts = new Dictionary<uint, BufferPool.Pool>();// TODO: avoid allocation
									var pool = bufferPool.GetAvaliable((int)header->dataSize);
									Array.Copy(data, dataRead, pool.data, 0, header->dataSize);
									//parts.Add(header->part, pool);
									recievingBuffers.Add(header->id, pool);
								}
								else if (!recievingBuffers[header->id].ContainsKey(header->part))// add part or ignore redundent packets
								{
									var pool = bufferPool.GetAvaliable((int)header->dataSize);
									Array.Copy(data, dataRead, pool.data, 0, header->dataSize);
									recievingBuffers[header->id].Add(header->part, pool);
								}*/
							}
						}
						else if (header->type == RUDPPacketType.Request)
						{
							/*if (recievingBuffers.ContainsKey(header->id))
							{
								if (!recievingBuffers[header->id].ContainsKey(header->part))// add part or ignore redundent packets
								{
									var pool = bufferPool.GetAvaliable((int)header->dataSize);
									Array.Copy(data, dataRead, pool.data, 0, header->dataSize);
									recievingBuffers[header->id].Add(header->part, pool);
								}
							}
							else// add new recieving buffer
							{
								var parts = new Dictionary<uint, BufferPool.Pool>();// TODO: avoid allocation
								var pool = bufferPool.GetAvaliable((int)header->dataSize);
								Array.Copy(data, dataRead, pool.data, 0, header->dataSize);
								parts.Add(header->part, pool);
								recievingBuffers.Add(header->id, parts);
							}*/
						}
						else if (header->type == RUDPPacketType.PacketRecievedResponse)
						{
							if (sendingBuffers.ContainsKey(header->id))
							{
								sendingBuffers[header->id].inUse = false;
								sendingBuffers.Remove(header->id);
							}
						}

						// always respond packet recieved to sender
						SendCustomPacket(header->id, null, RUDPPacketType.PacketRecievedResponse);
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

		private unsafe void SendCustomPacket(uint id, byte[] data, RUDPPacketType type)
		{
			if (data != null)
			{
				fixed (byte* dataPtr = data)
				{
					SendCustomPacket(id, dataPtr, data.Length, type);
				}
			}
			else
			{
				SendCustomPacket(id, null, 0, type);
			}
		}

		private unsafe void SendCustomPacket(uint id, byte* data, int dataSize, RUDPPacketType type)
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

		private int SendPacket(byte[] buffer, int offset, int size)
		{
			int headerSize = Marshal.SizeOf<RUPDPacketHeader>();

			// get avaliable pool
			var pool = bufferPool.GetAvaliable(headerSize + size);
			sendingBuffers.Add(nextPacketID, pool);

			// copy header & data into pool
			unsafe
			{
				var header = new RUPDPacketHeader(nextPacketID, (uint)size, RUDPPacketType.Request);
				fixed (byte* poolDataPtr = pool.data)
				fixed (byte* bufferPtr = buffer)
				{
					Buffer.MemoryCopy(&header, poolDataPtr, headerSize, headerSize);
					Buffer.MemoryCopy(bufferPtr, poolDataPtr + headerSize, size, size);
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

		public int Send(byte[] buffer)
		{
			lock (this) return SendPacket(buffer, 0, buffer.Length);
		}

		public int Send(byte[] buffer, int offset, int size)
		{
			lock (this) return SendPacket(buffer, offset, size);
		}

		public int Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			return Send(data);
		}
	}
}