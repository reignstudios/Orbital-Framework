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

		private uint nextPacketID, sendingPacketID, lastReceivedPacketID;
		private RUDPBufferPool.Pool[] sendingBuffers;
		private int senderingBuffersLength;

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

			sendingPacketID = uint.MaxValue;
			lastReceivedPacketID = uint.MaxValue;
			sendingBuffers = new RUDPBufferPool.Pool[100];
			tryToSendTimer = new Timer(SendWaitCallbackTimer, null, 0, 100);
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

			sendingBuffers = null;
			senderingBuffersLength = 0;

			socket.RemoveConnection(this);
			DataRecievedCallback = null;
			DisconnectedCallback?.Invoke(this);
			DisconnectedCallback = null;
		}

		internal unsafe void FireDataRecievedCallback(RUPDPacketHeader* header, byte[] data, int offset, int size)
		{
			if (lastReceivedPacketID != header->id)
			{
				DataRecievedCallback?.Invoke(this, data, offset, size);
				lastReceivedPacketID = header->id;
			}
		}

		internal unsafe void DataSentResponse(uint id)
		{
			lock (this)
			{
				if (isDisposed) return;
				if (sendingPacketID == id && senderingBuffersLength > 0)
				{
					// release pool
					sendingBuffers[0].inUse = false;

					// remove first
					senderingBuffersLength--;
					for (int i = 0; i != senderingBuffersLength; ++i) sendingBuffers[i] = sendingBuffers[i + 1];// shift buffer down

					// next sending packet waiting
					if (senderingBuffersLength != 0)
					{
						fixed (byte* data = sendingBuffers[0].data)
						{
							var header = (RUPDPacketHeader*)data;
							sendingPacketID = header->id;
						}
					}
					else
					{
						sendingPacketID = uint.MaxValue;
					}
				}
			}
		}

		public override bool IsConnected()
		{
			return isConnected;
		}

		private unsafe void SendWaitCallbackTimer(object state)
		{
			bool isDisconnected = false;
			lock (this)
			{
				if (isDisposed) return;
				if (senderingBuffersLength == 0) return;

				var now = DateTime.Now;
				var pool = sendingBuffers[0];
				if ((pool.usedAtTime - now).TotalSeconds >= 60)
				{
					isDisconnected = true;
				}
				else
				{
					fixed (byte* dataPtr = pool.data)
					{
						// try sending data again
						try
						{
							socket.udpSocket.Send(pool.data, 0, pool.usedDataSize, endPoint);
						}
						catch { }
					}
				}
			}

			if (isDisconnected) Dispose();
		}

		private unsafe int SendPacket(byte* buffer, int offset, int size)
		{
			// check if buffer is full, if so block until space avaliable
			while (true)
			{
				lock (this)
				{
					if (isDisposed) return 0;
					if (senderingBuffersLength < sendingBuffers.Length) break;
				}
			}

			// continue sending
			lock (this)
			{
				if (isDisposed) return 0;
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
				int bytesSent = 0;
				if (senderingBuffersLength == 0)// only send now if nothing in que
				{
					try
					{
						bytesSent = socket.udpSocket.Send(pool.data, offset, pool.usedDataSize, endPoint);
						sendingPacketID = nextPacketID;
					}
					catch (Exception e)
					{
						pool.inUse = false;
						throw e;
					}
				}

				// add packet to sending pool
				sendingBuffers[senderingBuffersLength] = pool;
				senderingBuffersLength++;

				// increment packet
				++nextPacketID;
				if (nextPacketID == uint.MaxValue) nextPacketID = 0;

				// return how many bytes sent
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
