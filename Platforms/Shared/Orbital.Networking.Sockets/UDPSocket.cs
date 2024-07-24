using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
	public class UDPSocket : Socket, INetworkDataSender
	{
		public delegate void DataRecievedCallbackMethod(UDPSocket socket, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(UDPSocket socket, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		protected NativeSocket udpSocket;
		protected EndPoint remoteEndPoint, localEndPoint;
		private IPAddress localAddress;
		private bool isConnected;
		public readonly bool isMulticast;

		protected readonly byte[] sendBuffer, receiveBuffer;
		protected bool recieveData, async;

		public UDPSocket(IPAddress remoteAddress, IPAddress localAddress, int port, bool isMulticast, int maxBufferSize, bool async = true)
		: base(remoteAddress, port)
		{
			this.localAddress = localAddress;
			this.isMulticast = isMulticast;
			this.async = async;

			remoteEndPoint = new IPEndPoint(remoteAddress, port);
			localEndPoint = new IPEndPoint(localAddress, port);

			sendBuffer = new byte[maxBufferSize];
			receiveBuffer = new byte[maxBufferSize];
		}

		/// <summary>
		/// Start attempting a connection
		/// </summary>
		/// <param name="recieveData">Whether or not to recieve data over this socket</param>
		public void Join(bool recieveData)
		{
			lock (this)
			{
				if (isDisposed || udpSocket != null) throw new Exception("Can only be called once");

				// join
				udpSocket = new NativeSocket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
				udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endpoint to be reused if Windows fails to close it on app quit

				if (isMulticast)
				{
					udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address, localAddress));
					if (IPAddress.IsLoopback(localAddress)) udpSocket.MulticastLoopback = true;
					udpSocket.Bind(localEndPoint);
				}
				else
				{
					if (!IPAddress.IsLoopback(localAddress) || recieveData) udpSocket.Bind(localEndPoint);
				}

				isConnected = true;

				// listen
				this.recieveData = recieveData && async;
				if (recieveData)
				{
					try
					{
						udpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, RecieveDataCallback, null);
					}
					catch (Exception e)
					{
						isConnected = false;
						throw e;
					}
				}
			}
		}

		public override void Dispose()
		{
			Dispose(null);
		}

		public void Dispose(string message)
		{
			base.Dispose();

			bool wasConnected;
			lock (this)
			{
				wasConnected = isConnected;
				isConnected = false;

				if (udpSocket != null)
				{
					udpSocket.Close();
					udpSocket.Dispose();
					udpSocket = null;
				}
			}

			if (wasConnected) DisconnectedCallback?.Invoke(this, message);
			DataRecievedCallback = null;
			DisconnectedCallback = null;
		}

		public override bool IsConnected()
		{
			lock (this) return isConnected;// && udpSocket != null && udpSocket.Connected;// NOTE: UDP sockets 'Connected' is always false
		}

		protected void RecieveDataCallback(IAsyncResult ar)
		{
			bool disconnected = false;
			int bytesRead = 0;
			lock (this)
			{
				if (isDisposed || !isConnected) return;

				// handle failed reads
				try
				{
					bytesRead = udpSocket.EndReceiveFrom(ar, ref remoteEndPoint);
				}
				catch
				{
					disconnected = true;
				}

				if (bytesRead <= 0) disconnected = true;
			}

			// fire data recieved callback
			if (!disconnected) DataRecievedCallback?.Invoke(this, receiveBuffer, bytesRead);

			// start waiting for more data
			lock (this)
			{
				if (isConnected && !disconnected)
				{
					try
					{
						udpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPoint, RecieveDataCallback, null);
					}
					catch
					{
						disconnected = true;
					}
				}
			}

			if (disconnected || !IsConnected()) Dispose("Disposed");
		}

		public unsafe int Send(byte* buffer, int size)
		{
			return Send(buffer, size, remoteEndPoint);
		}

		public unsafe int Send(byte* buffer, int size, EndPoint endpoint)
		{
			lock (this)
			{
				try
				{
					fixed (byte* sendBufferPtr = sendBuffer) Buffer.MemoryCopy(buffer, sendBufferPtr, size, size);
					return udpSocket.SendTo(sendBuffer, size, SocketFlags.None, endpoint);
				}
				catch (Exception e)
				{
					if (!IsConnected()) Dispose(e.Message);
					throw e;
				}
			}
		}

		public unsafe int Send(byte* buffer, int offset, int size)
		{
			return Send(buffer + offset, size);
		}

		public unsafe int Send(byte* buffer, int offset, int size, EndPoint endpoint)
		{
			return Send(buffer + offset, size, endpoint);
		}

		public unsafe int Send<T>(T* data) where T : unmanaged
		{
			return Send((byte*)data, Marshal.SizeOf<T>());
		}

		public unsafe int Send<T>(T* data, EndPoint endpoint) where T : unmanaged
		{
			return Send((byte*)data, Marshal.SizeOf<T>(), endpoint);
		}

		public int Send(byte[] buffer)
		{
			try
			{
				return udpSocket.SendTo(buffer, SocketFlags.None, remoteEndPoint);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public int Send(byte[] buffer, EndPoint endpoint)
		{
			try
			{
				return udpSocket.SendTo(buffer, SocketFlags.None, endpoint);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public int Send(byte[] buffer, int offset, int size)
		{
			try
			{
				return udpSocket.SendTo(buffer, offset, size, SocketFlags.None, remoteEndPoint);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public int Send(byte[] buffer, int offset, int size, EndPoint endpoint)
		{
			try
			{
				return udpSocket.SendTo(buffer, offset, size, SocketFlags.None, endpoint);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public int Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			return Send(data);
		}

		public int Recieve(byte[] recieveBuffer)
		{
			if (recieveData) throw new Exception("Socket cannot be async");
			try
			{
				return udpSocket.Receive(recieveBuffer);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}
	}
}
