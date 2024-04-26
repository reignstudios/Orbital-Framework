using System.Collections.Generic;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Orbital.Networking.Sockets
{
	public sealed class RUDPSocketConnection : Socket, INetworkDataSender
	{
		public RUDPSocket socket { get; private set; }
		public IPEndPoint endPoint { get; private set; }
		public readonly Guid addressID;
		private bool isConnected;
		private Timer tryToSendTimer;

		private uint nextPacketID;
		private Dictionary<uint, RUDPBufferPool.Pool> sendingBuffers;

		public delegate void DataRecievedCallbackMethod(RUDPSocketConnection connection, byte[] data, int offset, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(RUDPSocketConnection connection);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		public RUDPSocketConnection(RUDPSocket socket, IPAddress remoteAddress, Guid remoteAddressID, int port)
		: base(remoteAddress, port)
		{
			this.socket = socket;
			endPoint = new IPEndPoint(remoteAddress, port);
			addressID = remoteAddressID;
			sendingBuffers = new Dictionary<uint, RUDPBufferPool.Pool>();
			tryToSendTimer = new Timer(SendWaitCallbackTimer, null, 0, 500);
		}

		public override void Dispose()
		{
			base.Dispose();
			isConnected = false;

			if (tryToSendTimer != null)
			{
				tryToSendTimer.Dispose();
				tryToSendTimer = null;
			}

			DataRecievedCallback = null;
			DisconnectedCallback?.Invoke(this);
			DisconnectedCallback = null;
		}

		internal void FireDataRecievedCallback(byte[] data, int offset, int size)
		{
			DataRecievedCallback?.Invoke(this, data, offset, size);// TODO: keep track if duplicate packets already recieved and ignore them
		}

		internal void DataSentResponse(uint id)
		{
			lock (this)
			{
				if (sendingBuffers.ContainsKey(id))
				{
					sendingBuffers[id].inUse = false;
					sendingBuffers.Remove(id);
				}
			}
		}

		public override bool IsConnected()
		{
			return isConnected;
		}

		private unsafe void SendWaitCallbackTimer(object state)
		{
			lock (this)
			{
				foreach (var buffer in sendingBuffers)// TODO: guarantee packet order (only send next)
				{
					var pool = buffer.Value;
					fixed (byte* dataPtr = pool.data)
					{
						var header = (RUPDPacketHeader*)dataPtr;

						// try sending data again
						try
						{
							socket.udpSocket.Send(pool.data, 0, header->dataSize, endPoint);
						}
						catch { }
					}
				}
			}
		}

		private unsafe int SendPacket(byte* buffer, int offset, int size)
		{
			lock (this)
			{
				int headerSize = Marshal.SizeOf<RUPDPacketHeader>();

				// get avaliable pool
				var pool = socket.bufferPool.GetAvaliable(headerSize + size);

				// copy header & data into pool
				var header = new RUPDPacketHeader(nextPacketID, socket.senderAddressID, addressID, port, size, RUDPPacketType.Send);
				fixed (byte* poolDataPtr = pool.data)
				{
					Buffer.MemoryCopy(&header, poolDataPtr, headerSize, headerSize);
					if (buffer != null && size != 0) Buffer.MemoryCopy(buffer, poolDataPtr + headerSize, size, size);
				}

				// send packet
				int bytesSent;
				try
				{
					bytesSent = socket.udpSocket.Send(pool.data, offset, size, endPoint);// TODO: guarantee packet order (only send now if nothing in que)
				}
				catch (Exception e)
				{
					pool.inUse = false;
					throw e;
				}

				// increment packet
				++nextPacketID;
				if (nextPacketID == uint.MaxValue) nextPacketID = 0;// max-value is reserved for connection tests

				// return how many bytes sent
				sendingBuffers.Add(nextPacketID, pool);
				return bytesSent;
			}
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
