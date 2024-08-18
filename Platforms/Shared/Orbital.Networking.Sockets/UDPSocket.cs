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
		public IPAddress remoteAddress => address;
		public IPEndPoint remoteEndPoint => endPoint;
		public readonly IPAddress localAddress;
		public readonly IPEndPoint localEndPoint;
		public readonly bool isMulticast;
		public readonly bool async;
		private bool isConnected;

		protected readonly byte[] sendBuffer, receiveBuffer;
		protected bool recieveData;

		public UDPSocket(IPAddress remoteAddress, IPAddress localAddress, int port, bool isMulticast, int maxBufferSize, bool async = true)
		: base(remoteAddress, port)
		{
			this.localAddress = localAddress;
			localEndPoint = new IPEndPoint(localAddress, port);
			this.isMulticast = isMulticast;
			this.async = async;

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
						var remoteEndPointRef = (EndPoint)remoteEndPoint;
						udpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPointRef, RecieveDataCallback, null);
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

			if (wasConnected)
			{
				try
				{
					DisconnectedCallback?.Invoke(this, message);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					System.Diagnostics.Debug.WriteLine(e);
				}
			}
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
					var remoteEndPointRef = (EndPoint)remoteEndPoint;
					bytesRead = udpSocket.EndReceiveFrom(ar, ref remoteEndPointRef);
				}
				catch
				{
					disconnected = true;
				}

				if (bytesRead <= 0) disconnected = true;
			}

			// fire data recieved callback
			if (!disconnected)
			{
				try
				{
					DataRecievedCallback?.Invoke(this, receiveBuffer, bytesRead);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					System.Diagnostics.Debug.WriteLine(e);
				}
			}

			// start waiting for more data
			lock (this)
			{
				if (isConnected && !disconnected)
				{
					try
					{
						var remoteEndPointRef = (EndPoint)remoteEndPoint;
						udpSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref remoteEndPointRef, RecieveDataCallback, null);
					}
					catch
					{
						disconnected = true;
					}
				}
			}

			if (disconnected || !IsConnected()) Dispose("Disposed");
		}

		public unsafe void Send(byte* data, int size)
		{
			Send(data, size, remoteEndPoint);
		}

		public unsafe void Send(byte* data, int size, EndPoint endpoint)
		{
			lock (this)
			{
				try
				{
					fixed (byte* sendBufferPtr = sendBuffer) Buffer.MemoryCopy(data, sendBufferPtr, size, size);
					int sent = 0;
					do
					{
						sent += udpSocket.SendTo(sendBuffer, sent, size - sent, SocketFlags.None, endpoint);
					} while (sent < size);
				}
				catch (Exception e)
				{
					if (!IsConnected()) Dispose(e.Message);
					throw e;
				}
			}
		}

		public unsafe void Send(byte* data, int offset, int size)
		{
			Send(data + offset, size, remoteEndPoint);
		}

		public unsafe void Send(byte* data, int offset, int size, EndPoint endpoint)
		{
			Send(data + offset, size, endpoint);
		}
		
		public unsafe void Send<T>(T data) where T : unmanaged
		{
			Send((byte*)&data, Marshal.SizeOf<T>(), remoteEndPoint);
		}

		public unsafe void Send<T>(T data, EndPoint endpoint) where T : unmanaged
		{
			Send((byte*)&data, Marshal.SizeOf<T>(), endpoint);
		}

		public unsafe void Send<T>(T* data) where T : unmanaged
		{
			Send((byte*)data, Marshal.SizeOf<T>(), remoteEndPoint);
		}

		public unsafe void Send<T>(T* data, EndPoint endpoint) where T : unmanaged
		{
			Send((byte*)data, Marshal.SizeOf<T>(), endpoint);
		}

		public void Send(byte[] data)
		{
			Send(data, 0, data.Length, remoteEndPoint);
		}

		public void Send(byte[] data, EndPoint endpoint)
		{
			Send(data, 0, data.Length, endpoint);
		}

		public void Send(byte[] data, int size)
		{
			Send(data, 0, size, remoteEndPoint);
		}

		public void Send(byte[] data, int size, EndPoint endpoint)
		{
			Send(data, 0, size, endpoint);
		}

		public void Send(byte[] data, int offset, int size)
		{
			Send(data, offset, size, remoteEndPoint);
		}

		public void Send(byte[] data, int offset, int size, EndPoint endpoint)
		{
			try
			{
				int sent = 0;
				do
				{
					sent += udpSocket.SendTo(data, offset + sent, size - sent, SocketFlags.None, endpoint);
				} while (sent < size);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public void Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			Send(data);
		}

		public void Send(string text, Encoding encoding, EndPoint endpoint)
		{
			byte[] data = encoding.GetBytes(text);
			Send(data, endpoint);
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
