using System;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using NativeSocket = System.Net.Sockets.Socket;

namespace Orbital.Networking.Sockets
{
	public sealed class TCPSocketConnection : INetworkDataSender, INetworkDataReceiver, IDisposable
	{
		public delegate void DataRecievedCallbackMethod(TCPSocketConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(TCPSocketConnection connection, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		public delegate void GeneralErrorCallbackMethod(TCPSocketConnection connection, Exception e);
		public event GeneralErrorCallbackMethod GeneralErrorCallback;

		private NativeSocket nativeSocket;
		public readonly TCPSocket socket;
		public readonly IPAddress address;
		public readonly Guid addressID;
		public readonly int port;
		public readonly bool async;
		public readonly IPEndPoint endPoint;
		public readonly PhysicalAddress physicalAddress;
		private bool isConnected;

		private const int receiveBufferSize = 1024;
		private readonly byte[] receiveBuffer;
		private byte[] sendBuffer;

		private Thread thread;
		private bool threadAlive;

		public TCPSocketConnection(TCPSocket socket, NativeSocket nativeSocket, IPAddress address, int port, IPAddress localAddress, bool async)
		{
			this.socket = socket;
			this.nativeSocket = nativeSocket;
			this.address = address;
			addressID = Socket.AddressToAddressID(address);
			this.port = port;
			this.async = async;

			endPoint = new IPEndPoint(address, port);
			physicalAddress = SocketUtils.GetMacAddress(localAddress);
			if (async) receiveBuffer = new byte[receiveBufferSize];

			isConnected = true;
		}

		internal static void Dispose(NativeSocket socket)
		{
			try
			{
				if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
			} catch { }

			try
			{
				socket.Close();
			} catch { }

			try
			{
				socket.Dispose();
			} catch { }
		}

		public void Dispose()
		{
			Dispose((string)null);
		}

		public void Dispose(string message)
		{
			threadAlive = false;
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

		internal void Init()
		{
			if (async)
			{
				thread = new Thread(AsyncThread);
				thread.IsBackground = true;
				threadAlive = true;
				thread.Start();
			}
		}

		private void AsyncThread(object obj)
		{
			while (threadAlive && isConnected)
			{
				int size = 0;
				try
				{
					size = nativeSocket.Receive(receiveBuffer);
				}
				catch (Exception e)
				{
					GeneralErrorCallback?.Invoke(this, e);
					Dispose(e.Message);
				}

				try
				{
					if (size > 0) DataRecievedCallback?.Invoke(this, receiveBuffer, size);
				}
				catch (Exception e)
				{
					GeneralErrorCallback?.Invoke(this, e);
				}
			}

			threadAlive = false;
		}

		internal static bool IsConnected(NativeSocket socket)
		{
			return socket != null && socket.Connected;
		}

		public bool IsConnected()
		{
			lock (this) return isConnected && IsConnected(nativeSocket);
		}

		public unsafe void Send(byte* data, int size)
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
					sent += nativeSocket.Send(sendBuffer, sent, size - sent, SocketFlags.None);
				} while (sent < size);
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public unsafe void Send(byte* data, int offset, int size)
		{
			Send(data + offset, size);
		}

		public unsafe void Send<T>(T data) where T : unmanaged
		{
			Send((byte*)&data, Marshal.SizeOf<T>());
		}

		public unsafe void Send<T>(T* data) where T : unmanaged
		{
			Send((byte*)data, Marshal.SizeOf<T>());
		}

		public void Send(byte[] data)
		{
			Send(data, 0, data.Length);
		}

		public void Send(byte[] data, int size)
		{
			Send(data, 0, size);
		}
		
		public void Send(byte[] data, int offset, int size)
		{
			try
			{
				int sent = 0;
				do
				{
					sent += nativeSocket.Send(data, offset + sent, size - sent, SocketFlags.None);
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
						int sent = 0;
						do
						{
							sent += nativeSocket.Send(buffer, 0, read, SocketFlags.None);
							if (callback != null)
							{
								fileSent += sent;
								int percent = (int)((fileSent / (float)fileSize) * 100);
								if (lastPercent != percent) callback(percent);
								lastPercent = percent;
							}
						} while (sent < read);
					}
				} while (read > 0);

				return fileSent;
			}
			catch (Exception e)
			{
				if (!IsConnected()) Dispose(e.Message);
				throw e;
			}
		}

		public bool ReceiveSyncReady()
		{
			return nativeSocket.Poll(0, SelectMode.SelectRead);
		}

		public int ReceiveSync(byte[] receiveBuffer)
		{
			if (async) throw new Exception("Socket cannot be async");
			try
			{
				int size = nativeSocket.Receive(receiveBuffer);
				if (size > 0) DataRecievedCallback?.Invoke(this, receiveBuffer, size);
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
