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
		private bool isConnected;
		private Timer tryToSendTimer;

		private uint nextPacketID, sendingPacketID, lastReceivedPacketID;
		private RUDPBufferPool.Pool[] sendingBuffers;
		private int senderingBuffersLength;

		public delegate void DataRecievedCallbackMethod(RUDPSocketConnection connection, byte[] data, int offset, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(RUDPSocketConnection connection, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		public RUDPSocketConnection(RUDPSocket socket, IPAddress remoteAddress, Guid remoteAddressID, int port)
		: base(remoteAddress, port)
		{
			this.socket = socket;
			sendingPacketID = uint.MaxValue;
			lastReceivedPacketID = uint.MaxValue;
			sendingBuffers = new RUDPBufferPool.Pool[100];
			tryToSendTimer = new Timer(SendWaitCallbackTimer, null, 0, 100);
		}

		public override void Dispose()
		{
			Dispose(null);
		}

		public void Dispose(string message)
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
			try
			{
				DisconnectedCallback?.Invoke(this, message);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				System.Diagnostics.Debug.WriteLine(e);
			}
			DataRecievedCallback = null;
			DisconnectedCallback = null;
		}

		internal unsafe void FireDataRecievedCallback(RUDPPacketHeader* header, byte[] data, int offset, int size)
		{
			if (lastReceivedPacketID != header->id)
			{
				lastReceivedPacketID = header->id;
				try
				{
					DataRecievedCallback?.Invoke(this, data, offset, size);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					System.Diagnostics.Debug.WriteLine(e);
				}
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
					for (int i = 0; i < senderingBuffersLength; ++i) sendingBuffers[i] = sendingBuffers[i + 1];// shift buffer down

					// next sending packet waiting
					if (senderingBuffersLength != 0)
					{
						fixed (byte* data = sendingBuffers[0].data)
						{
							var header = (RUDPPacketHeader*)data;
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
				if (socket.timeout >= 0 && (now - pool.usedAtTime).TotalSeconds >= socket.timeout)
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

			if (isDisconnected) Dispose("Disconnected");
		}

		private unsafe void SendPacket(byte* data, int offset, int size)
		{
			// check if buffer is full, if so block until space avaliable
			while (true)
			{
				lock (this)
				{
					if (isDisposed) return;
					if (senderingBuffersLength < sendingBuffers.Length) break;
				}
			}

			// continue sending
			lock (this)
			{
				if (isDisposed) return;
				int headerSize = Marshal.SizeOf<RUDPPacketHeader>();

				// get avaliable pool
				var pool = socket.bufferPool.GetAvaliable(headerSize + size);

				// copy header & data into pool
				var header = new RUDPPacketHeader(nextPacketID, socket.senderAddressID, addressID, port, size, RUDPPacketType.Send);
				fixed (byte* poolDataPtr = pool.data)
				{
					Buffer.MemoryCopy(&header, poolDataPtr, headerSize, headerSize);
					if (data != null && size != 0) Buffer.MemoryCopy(data, poolDataPtr + headerSize, size, size);
				}

				// send packet
				int bytesSent = 0;
				if (senderingBuffersLength == 0)// only send now if nothing in que
				{
					try
					{
						if (socket.useBurst)
						{
							for (int i = 0; i < socket.burstCount; ++i)
							{
								socket.udpSocket.Send(pool.data, offset, pool.usedDataSize, endPoint);
							}
						}
						else
						{
							socket.udpSocket.Send(pool.data, offset, pool.usedDataSize, endPoint);
						}
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
			}
		}

		public unsafe void Send(byte* data, int size)
		{
			SendPacket(data, 0, size);
		}

		public unsafe void Send(byte* data, int offset, int size)
		{
			SendPacket(data, offset, size);
		}

		public unsafe void Send<T>(T data) where T : unmanaged
		{
			Send((byte*)&data, Marshal.SizeOf<T>());
		}

		public unsafe void Send<T>(T* data) where T : unmanaged
		{
			Send((byte*)data, Marshal.SizeOf<T>());
		}

		public unsafe void Send(byte[] data)
		{
			fixed (byte* dataPtr = data) SendPacket(dataPtr, 0, data.Length);
		}

		public unsafe void Send(byte[] data, int size)
		{
			fixed (byte* dataPtr = data) SendPacket(dataPtr, 0, size);
		}
		
		public unsafe void Send(byte[] data, int offset, int size)
		{
			fixed (byte* dataPtr = data) SendPacket(dataPtr, offset, size);
		}

		public void Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			Send(data);
		}
	}
}
