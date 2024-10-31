using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
	public sealed class UDPSocket : Socket, INetworkDataSender, INetworkDataReceiver, IDisposable
    {
		public delegate void DataRecievedCallbackMethod(UDPSocket socket, byte[] data, int size, IPEndPoint endPoint);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(UDPSocket socket, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		public delegate void GeneralErrorCallbackMethod(UDPSocket socket, Exception e);
		public event GeneralErrorCallbackMethod GeneralErrorCallback;

		private NativeSocket udpSocket;
		public IPAddress remoteAddress => address;
		public IPEndPoint remoteEndPoint => endPoint;
		public readonly IPAddress localAddress;
		public readonly IPEndPoint localEndPoint;
		public readonly bool isMulticast;
		public readonly bool async;
		private readonly bool captureAsyncReceiverEndPoint;
		private bool isConnected;

		private readonly byte[] receiveBuffer;
		private byte[] sendBuffer;
		
		private Thread thread;
		private bool threadAlive;

        /// <summary>
        /// UDPSocket
        /// </summary>
        /// <param name="remoteAddress">Remote address we want to send packets to</param>
        /// <param name="localAddress">Local endPoint we want to connect from</param>
        /// <param name="port">Port we want to send & recieve from</param>
        /// <param name="isMulticast">Remote address becomes multicast address</param>
        /// <param name="async">Use threads</param>
        /// <param name="asyncBufferSize">Buffer size for recieving async packet data</param>
        /// <param name="captureAsyncReceiverEndPoint">Capture the IPEndPoint in async mode</param>
        /// <param name="bindLocalEndPoint">Bind local endPoint address</param>
        public UDPSocket(IPAddress remoteAddress, IPAddress localAddress, int port, bool isMulticast, bool async, int asyncBufferSize, bool captureAsyncReceiverEndPoint = false)
		: base(remoteAddress, port)
		{
			this.localAddress = localAddress;
			localEndPoint = new IPEndPoint(localAddress, port);
			this.isMulticast = isMulticast;
			this.async = async;
			this.captureAsyncReceiverEndPoint = captureAsyncReceiverEndPoint;
			if (async) receiveBuffer = new byte[asyncBufferSize];
		}
		
		public override void Dispose()
		{
			Dispose(null);
		}

		public void Dispose(string message)
		{
			base.Dispose();
			threadAlive = false;

			bool wasConnected;
			lock (this)
			{
				wasConnected = isConnected;
				isConnected = false;

				if (udpSocket != null)
				{
					try
					{
						udpSocket.Shutdown(SocketShutdown.Both);
					} catch { }

					try
					{
						udpSocket.Close();
					} catch { }

					try
					{
						udpSocket.Dispose();
					} catch { }

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
					GeneralErrorCallback?.Invoke(this, e);
				}
			}
			DataRecievedCallback = null;
			DisconnectedCallback = null;
		}

		/// <summary>
		/// Start attempting a connection
		/// </summary>
		public void Join()
		{
			lock (this)
			{
				if (isDisposed || udpSocket != null) throw new Exception("Can only be called once");

				// join
				udpSocket = new NativeSocket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
				udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
				udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);// this allows the endPoint to be reused if Windows fails to close it on app quit

                if (isMulticast)
				{
					udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(address, localAddress));
					if (IPAddress.IsLoopback(localAddress)) udpSocket.MulticastLoopback = true;
					udpSocket.Bind(localEndPoint);
				}
				else
				{
					if (!IPAddress.IsLoopback(localAddress)) udpSocket.Bind(localEndPoint);
				}

				isConnected = true;

				// listen
				if (async)
				{
					thread = new Thread(AsyncThread);
					thread.IsBackground = true;
					threadAlive = true;
					thread.Start();
				}
			}
		}

		private void AsyncThread(object obj)
		{
			IPEndPoint remoteEndPointIP = null;
			EndPoint remoteEndPoint = null;
			if (captureAsyncReceiverEndPoint)
			{
				remoteEndPointIP = new IPEndPoint(IPAddress.Any, 0);
				remoteEndPoint = remoteEndPointIP;
			}

			while (threadAlive && isConnected && !isDisposed)
			{
				int size = 0;
				try
				{
					if (captureAsyncReceiverEndPoint)
					{
						//remoteEndPointIP.Address = IPAddress.Any;// NOTE: don't reset these or it can cause allocations when not needed
						//remoteEndPointIP.Port = 0;
						remoteEndPoint = remoteEndPointIP;
						size = udpSocket.ReceiveFrom(receiveBuffer, ref remoteEndPoint);// NOTE: this causes an alloc each time its called (maybe this could be improved)
						remoteEndPointIP = (IPEndPoint)remoteEndPoint;
					}
					else
					{
						size = udpSocket.Receive(receiveBuffer);
					}
				}
				catch (Exception e)
				{
					GeneralErrorCallback?.Invoke(this, e);
					Dispose(e.Message);
				}

				try
				{
					if (size > 0) DataRecievedCallback?.Invoke(this, receiveBuffer, size, remoteEndPointIP);
				}
				catch (Exception e)
				{
					GeneralErrorCallback?.Invoke(this, e);
				}
			}
			
			threadAlive = false;
		}

		public override bool IsConnected()
		{
			lock (this) return isConnected;// && udpSocket != null && udpSocket.Connected;// NOTE: UDP sockets 'Connected' is always false
		}

		public unsafe void Send(byte* data, int size)
		{
			Send(data, size, remoteEndPoint);
		}

		public unsafe void Send(byte* data, int size, EndPoint endPoint)
		{
			lock (this)
			{
				try
				{
					// validate send buffer is correct size
					if (sendBuffer == null) sendBuffer = new byte[size];
					else if (sendBuffer.Length < size) Array.Resize(ref sendBuffer, size);
					
					// send
					fixed (byte* sendBufferPtr = sendBuffer) Buffer.MemoryCopy(data, sendBufferPtr, size, size);
					int sent = 0;
					do
					{
						sent += udpSocket.SendTo(sendBuffer, sent, size - sent, SocketFlags.None, endPoint);
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

		public unsafe void Send(byte* data, int offset, int size, EndPoint endPoint)
		{
			Send(data + offset, size, endPoint);
		}
		
		public unsafe void Send<T>(T data) where T : unmanaged
		{
			Send((byte*)&data, Marshal.SizeOf<T>(), remoteEndPoint);
		}

		public unsafe void Send<T>(T data, EndPoint endPoint) where T : unmanaged
		{
			Send((byte*)&data, Marshal.SizeOf<T>(), endPoint);
		}

		public unsafe void Send<T>(T* data) where T : unmanaged
		{
			Send((byte*)data, Marshal.SizeOf<T>(), remoteEndPoint);
		}

		public unsafe void Send<T>(T* data, EndPoint endPoint) where T : unmanaged
		{
			Send((byte*)data, Marshal.SizeOf<T>(), endPoint);
		}

		public void Send(byte[] data)
		{
			Send(data, 0, data.Length, remoteEndPoint);
		}

		public void Send(byte[] data, EndPoint endPoint)
		{
			Send(data, 0, data.Length, endPoint);
		}

		public void Send(byte[] data, int size)
		{
			Send(data, 0, size, remoteEndPoint);
		}

		public void Send(byte[] data, int size, EndPoint endPoint)
		{
			Send(data, 0, size, endPoint);
		}

		public void Send(byte[] data, int offset, int size)
		{
			Send(data, offset, size, remoteEndPoint);
		}

		public void Send(byte[] data, int offset, int size, EndPoint endPoint)
		{
			try
			{
				int sent = 0;
				do
				{
					sent += udpSocket.SendTo(data, offset + sent, size - sent, SocketFlags.None, endPoint);
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

		public void Send(string text, Encoding encoding, EndPoint endPoint)
		{
			byte[] data = encoding.GetBytes(text);
			Send(data, endPoint);
		}

		public bool ReceiveSyncReady()
		{
			return udpSocket.Poll(0, SelectMode.SelectRead);
		}

		public int ReceiveSync(byte[] receiveBuffer)
		{
			if (async) throw new Exception("Socket cannot be async");
			try
			{
				int size = udpSocket.Receive(receiveBuffer);
				if (size > 0) DataRecievedCallback?.Invoke(this, receiveBuffer, size, null);
				return size;
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}
		
		public int ReceiveSync(byte[] receiveBuffer, ref IPEndPoint endPoint)
		{
			if (async) throw new Exception("Socket cannot be async");
			try
			{
				var ep = (EndPoint)endPoint;
				int size = udpSocket.ReceiveFrom(receiveBuffer, ref ep);
				endPoint = (IPEndPoint)ep;
				if (size > 0) DataRecievedCallback?.Invoke(this, receiveBuffer, size, endPoint);
				return size;
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}
	}
}
