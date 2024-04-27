using System;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

using NativeSocket = System.Net.Sockets.Socket;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Orbital.Networking.Sockets
{
	public sealed class TCPSocketConnection : INetworkDataSender, IDisposable
	{
		public delegate void DataRecievedCallbackMethod(TCPSocketConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(TCPSocketConnection connection);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		private NativeSocket nativeSocket;
		public readonly TCPSocket socket;
		public readonly IPAddress address;
		public readonly int port;
		private readonly bool async;
		public readonly PhysicalAddress physicalAddress;
		private bool isConnected;

		private const int receiveBufferSize = 1024;
		private readonly byte[] receiveBuffer;
		private byte[] sendBuffer;

		public TCPSocketConnection(TCPSocket socket, NativeSocket nativeSocket, IPAddress address, int port, IPAddress localAddress, bool async)
		{
			this.socket = socket;
			this.nativeSocket = nativeSocket;
			this.address = address;
			this.port = port;
			this.async = async;

			physicalAddress = SocketUtils.GetMacAddress(localAddress);
			if (async) receiveBuffer = new byte[receiveBufferSize];
			isConnected = true;
		}

		internal static void Dispose(NativeSocket socket)
		{
			// NOTE: Keep for reference.
			// Don't invoke these.
			// If the remote socket closes before the local one does it can prevent Windows from releasing the socket for reuse.
			// Close then Dispose seems to release socket correctly without these.
            /*try
            {
                if (IsConnected(socket))
				{
					socket.Shutdown(SocketShutdown.Both);
					socket.Disconnect(false);
				}
            }
            catch { }*/

            socket.Close();
            socket.Dispose();
		}

		public void Dispose()
		{
			bool wasConnected;
			lock (this)
			{
				wasConnected = isConnected;
				isConnected = false;

				if (nativeSocket != null)
				{
					Dispose(nativeSocket);
					nativeSocket = null;
				}
			}

			socket.RemoveConnection(this);
			if (wasConnected) DisconnectedCallback?.Invoke(this);
			DataRecievedCallback = null;
			DisconnectedCallback = null;
		}

		internal void InitRecieve()
		{
			lock (this)
			{
				if (!nativeSocket.Connected) throw new Exception("Socket not connected for recieving data");
				nativeSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, null);
			}
		}

		private void RecieveDataCallback(IAsyncResult ar)
		{
			bool disconnected = false;
			int bytesRead = 0;
			lock (this)
			{
				if (!isConnected) return;

				// handle failed reads
				try
				{
					bytesRead = nativeSocket.EndReceive(ar);
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
						nativeSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, RecieveDataCallback, null);
					}
					catch
					{
						disconnected = true;
					}
				}
			}

			if (disconnected || !IsConnected()) Dispose();
		}

		internal static bool IsConnected(NativeSocket socket)
		{
			return socket != null && socket.Connected;
		}

		public bool IsConnected()
		{
			lock (this) return isConnected && IsConnected(nativeSocket);
		}

		public unsafe int Send(byte* buffer, int size)
		{
			try
			{
				// validate send buffer is correct size
				if (sendBuffer == null) sendBuffer = new byte[size];
				else if (sendBuffer.Length < size) Array.Resize(ref sendBuffer, size);

				// send
				fixed (byte* sendBufferPtr = sendBuffer) Buffer.MemoryCopy(buffer, sendBufferPtr, size, size);
				return nativeSocket.Send(sendBuffer, size, SocketFlags.None);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose();
				throw e;
			}
		}

		public unsafe int Send(byte* buffer, int offset, int size)
		{
			return Send(buffer + offset, size);
		}

		public unsafe int Send<T>(T* data) where T : unmanaged
		{
			return Send((byte*)data, Marshal.SizeOf<T>());
		}

		public int Send(byte[] buffer)
		{
			try
			{
				return nativeSocket.Send(buffer, SocketFlags.None);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose();
				throw e;
			}
		}

		public int Send(byte[] buffer, int offset, int size)
		{
			try
			{
				return nativeSocket.Send(buffer, offset, size, SocketFlags.None);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose();
				throw e;
			}
		}

		public int Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			int sent = 0;
			do
			{
				sent += Send(data, sent, data.Length - sent);
			} while (sent < data.Length);
			return data.Length;
		}

		public delegate void SendFilePercentCallbackMethod(int percent);
		public long SendStream(Stream stream, SendFilePercentCallbackMethod callback = null)
		{
			try
			{
				var buffer = new byte[1024 * 8];
				long fileSize = stream.Length, fileSent = 0;
				int read = 0, lastPercent = 0;
				do
				{
					read = stream.Read(buffer, 0, buffer.Length);
					if (read > 0)
					{
						nativeSocket.Send(buffer, 0, read, SocketFlags.None);
						if (callback != null)
						{
							fileSent += read;
							int percent = (int)((fileSent / (float)fileSize) * 100);
							if (lastPercent != percent) callback(percent);
							lastPercent = percent;
						}
					}
				} while (read > 0);

				return fileSent;
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose();
				throw e;
			}
		}

		public int Recieve(byte[] recieveBuffer)
		{
			if (async) throw new Exception("Socket cannot be async");
			return nativeSocket.Receive(recieveBuffer);
		}
	}
}
