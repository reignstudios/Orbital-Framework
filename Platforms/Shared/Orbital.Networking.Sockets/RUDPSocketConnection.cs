using System.Collections.Generic;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Drawing;

namespace Orbital.Networking.Sockets
{
	public sealed class RUDPSocketConnection : Socket, INetworkDataSender, IDisposable
    {
		public RUDPSocket socket { get; private set; }
		private bool isConnected;
		private Timer tryToSendTimer;

		private uint nextPacketID, sendingPacketID, lastReceivedPacketID;
		private RUDPBufferPool.Pool[] sendingBuffers;
		private uint[] sendingBufferIDs;
		private int sendingBuffersLength;

		private uint nextReceivingPacketID;
		private Dictionary<uint, RUDPBufferPool.Pool> receiveBuffers;

        public readonly bool async;

        public delegate void DataRecievedCallbackMethod(RUDPSocketConnection connection, byte[] data, int offset, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(RUDPSocketConnection connection, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		public delegate void GeneralErrorCallbackMethod(RUDPSocketConnection connection, Exception e);
		public event GeneralErrorCallbackMethod GeneralErrorCallback;

		public RUDPSocketConnection(RUDPSocket socket, IPAddress remoteAddress, Guid remoteAddressID, int port, bool async)
		: base(remoteAddress, port)
		{
			this.socket = socket;
			this.async = async;
			sendingPacketID = uint.MaxValue;
			lastReceivedPacketID = uint.MaxValue;
			sendingBuffers = new RUDPBufferPool.Pool[100];
			sendingBufferIDs = new uint[100];
			if (socket.mode == RUDPMode.Blast) receiveBuffers = new Dictionary<uint, RUDPBufferPool.Pool>();
			tryToSendTimer = new Timer(SendWaitCallbackTimer, null, 0, 100);
			isConnected = true;// connected when created
		}

		public override void Dispose()
		{
			Dispose(null);
		}

		public unsafe void Dispose(string message)
		{
			DisposeInternal(message, true);
		}

		public unsafe void DisposeInternal(string message, bool removeConnection)
		{
			base.Dispose();
			isConnected = false;

			if (tryToSendTimer != null)
			{
				tryToSendTimer.Dispose();
				tryToSendTimer = null;
			}

			sendingBuffers = null;
			sendingBuffersLength = 0;
			receiveBuffers = null;

			if (removeConnection) socket.RemoveConnection(this);
			try
			{
				DisconnectedCallback?.Invoke(this, message);
			}
			catch (Exception e)
			{
				GeneralErrorCallback?.Invoke(this, e);
			}
			DataRecievedCallback = null;
			DisconnectedCallback = null;
		}

		internal unsafe void InvokeDataRecievedCallback(RUDPPacketHeader* header, byte[] data, int offset, int size)
		{
			if (socket.mode == RUDPMode.Blast)
			{
				// this is the next packet wanted
				if (header->id == nextReceivingPacketID)
				{
					try
					{
						DataRecievedCallback?.Invoke(this, data, offset, size);
					}
					catch (Exception e)
					{
						GeneralErrorCallback?.Invoke(this, e);
					}

					nextReceivingPacketID++;// next ID wanted
					if (nextReceivingPacketID == uint.MaxValue) nextReceivingPacketID = 0;
					if (receiveBuffers.ContainsKey(header->id)) receiveBuffers.Remove(header->id);// cleanup
				}

				// newer than wanted packet, so add to que pool
				else if (header->id > nextReceivingPacketID && !receiveBuffers.ContainsKey(header->id))
				{
					var pool = socket.bufferPool.GetAvaliable(size);
					Array.Copy(data, offset, pool.data, 0, size);
					receiveBuffers.Add(header->id, pool);
					return;// skip other work
				}
				
				// process other packets that could now be ready
				while (true)
				{
					if (receiveBuffers.ContainsKey(nextReceivingPacketID))
					{
						var pool = receiveBuffers[nextReceivingPacketID];
						try
						{
							DataRecievedCallback?.Invoke(this, pool.data, 0, pool.usedDataSize);
						}
						catch (Exception e)
						{
							GeneralErrorCallback?.Invoke(this, e);
						}

						receiveBuffers.Remove(nextReceivingPacketID);// cleanup
						nextReceivingPacketID++;// next ID wanted
						if (nextReceivingPacketID == uint.MaxValue) nextReceivingPacketID = 0;
						continue;// check if any more packets are ready
					}

					break;
				}
			}
			else
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
						GeneralErrorCallback?.Invoke(this, e);
					}
				}
			}
		}

		internal unsafe void DataSentResponse(uint id)
		{
			lock (this)
			{
				if (isDisposed) return;

				if (socket.mode == RUDPMode.Blast)
				{
					for (int i = 0; i < sendingBuffersLength; ++i)
					{
						if (sendingBufferIDs[i] == id)
						{
							// release pool
							sendingBuffers[i].inUse = false;

							// remove
							sendingBuffersLength--;
							for (int i2 = i; i2 < sendingBuffersLength; ++i2)// shift buffer down
							{
								sendingBuffers[i2] = sendingBuffers[i2 + 1];
								sendingBufferIDs[i2] = sendingBufferIDs[i2 + 1];
							}

							break;
						}
					}
				}
				else
				{
					if (sendingPacketID == id && sendingBuffersLength > 0)
					{
						// release pool
						sendingBuffers[0].inUse = false;

						// remove first
						sendingBuffersLength--;
						for (int i = 0; i < sendingBuffersLength; ++i) sendingBuffers[i] = sendingBuffers[i + 1];// shift buffer down

						// next sending packet waiting
						if (sendingBuffersLength != 0)
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
				if (sendingBuffersLength == 0) return;

				var now = DateTime.Now;
				for (int i = 0; i < sendingBuffersLength; ++i)
				{
					// grab pool
					RUDPBufferPool.Pool pool;
					if (socket.mode == RUDPMode.Blast) pool = sendingBuffers[i];
					else pool = sendingBuffers[0];

					// check for disconnection timeouts
					if (socket.timeout >= 0 && (now - pool.usedAtTime).TotalSeconds >= socket.timeout)
					{
						isDisconnected = true;
					}
					else
					{
						// try sending data again
						fixed (byte* dataPtr = pool.data)
						{
							try
							{
								socket.udpSocket.Send(pool.data, 0, pool.usedDataSize, endPoint);
							}
							catch { }
						}
					}

					// only send first pool with stream mode
					if (socket.mode == RUDPMode.Stream) break;
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
					if (sendingBuffersLength < sendingBuffers.Length) break;
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
				if (sendingBuffersLength == 0)// only send now if nothing in que
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
						if (!IsConnected()) Dispose(e.Message);
						throw e;
					}
				}

				// add packet to sending pool
				sendingBuffers[sendingBuffersLength] = pool;
				sendingBufferIDs[sendingBuffersLength] = sendingPacketID;
				sendingBuffersLength++;

				// increment packet
				nextPacketID++;
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
