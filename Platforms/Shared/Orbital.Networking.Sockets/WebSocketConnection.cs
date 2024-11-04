using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;

using NativeSocket = System.Net.WebSockets.WebSocket;
using System.Drawing;

namespace Orbital.Networking.Sockets
{
    public sealed class WebSocketConnection : INetworkDataSender, IDisposable
    {
        public delegate void DataRecievedCallbackMethod(WebSocketConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(WebSocketConnection connection, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		public delegate void GeneralErrorCallbackMethod(WebSocketConnection connection, Exception e);
		public event GeneralErrorCallbackMethod GeneralErrorCallback;

		private NativeSocket nativeSocket;
		public readonly WebSocket socket;
		public readonly string address;
		public readonly bool async;
		private bool isConnected;

		private const int receiveBufferSize = 1024;
		private readonly byte[] receiveBuffer;
		private byte[] sendBuffer;

		private Thread thread;
		private bool taskAlive;
		private uint sendTaskNext, waitSendTask;

		public WebSocketConnection(WebSocket socket, NativeSocket nativeSocket, string address, bool async)
		{
			this.socket = socket;
			this.nativeSocket = nativeSocket;
			this.address = address;
			this.async = async;
			isConnected = true;
		}

		internal static async Task Dispose(NativeSocket socket)
		{
			try
			{
				using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
				{
					await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Dispose", tokenSource.Token);
				}
			} catch { }

			try
			{
				socket.Dispose();
			} catch { }
		}

		public async void Dispose()
		{
			await DisposeTask(null);
		}

		private async void Dispose(string message)
		{
			await DisposeTask(message);
		}

		public async Task DisposeTask()
		{
			await DisposeTask(null);
		}

		private async Task DisposeTask(string message)
		{
			taskAlive = false;
			bool wasConnected;
			wasConnected = isConnected;
			isConnected = false;

			if (nativeSocket != null)
			{
				await Dispose(nativeSocket);
				nativeSocket = null;
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

		internal async void Init()
		{
			if (async)
			{
				taskAlive = true;
				await AsyncTask();
			}
		}

		private async Task AsyncTask()
		{
			while (taskAlive && isConnected)
			{
				int size = 0;
				try
				{
					var result = await nativeSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
					size = result.Count;
				}
				catch (Exception e)
				{
					GeneralErrorCallback?.Invoke(this, e);
					await DisposeTask(e.Message);
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

			taskAlive = false;
		}

		internal static bool IsConnected(NativeSocket socket)
		{
			return socket != null && socket.State != WebSocketState.Open;
		}

		public bool IsConnected()
		{
			lock (this) return isConnected && IsConnected(nativeSocket);
		}

		private async void SendAsync(byte[] data, int offset, int size, uint thisTask)
        {
			while (waitSendTask != thisTask) await Task.Yield();// ensure data is sent in order
			using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(socket.timeout)))
			{
				await nativeSocket.SendAsync(new ArraySegment<byte>(sendBuffer, offset, size), WebSocketMessageType.Binary, true, tokenSource.Token);
			}
			waitSendTask++;
			if (waitSendTask == uint.MaxValue) waitSendTask = 0;
		}

		private void IncromentSendTaskNext()
		{
			sendTaskNext++;
			if (sendTaskNext == uint.MaxValue) sendTaskNext = 0;
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
				SendAsync(sendBuffer, 0, size, sendTaskNext);
				IncromentSendTaskNext();
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
				SendAsync(data, offset, size, sendTaskNext);
				IncromentSendTaskNext();
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
		public async Task<long> SendStream(Stream stream, SendFilePercentCallbackMethod callback = null)
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
							using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(socket.timeout)))
							{
								await nativeSocket.SendAsync(new ArraySegment<byte>(buffer, 0, read), WebSocketMessageType.Binary, true, tokenSource.Token);
							}
							sent += read;
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
				if (!IsConnected()) await DisposeTask(e.Message);
				throw e;
			}
		}

		public async Task<int> ReceiveSync(byte[] receiveBuffer)
		{
			if (async) throw new Exception("Socket cannot be async");
			try
			{
				var result = await nativeSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
				if (result.Count > 0) DataRecievedCallback?.Invoke(this, receiveBuffer, result.Count);
				return result.Count;
			}
            catch (Exception e)
            {
                if (!IsConnected()) await DisposeTask(e.Message);
                throw e;
            }
        }
    }
}