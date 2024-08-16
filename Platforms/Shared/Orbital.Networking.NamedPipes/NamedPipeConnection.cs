using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Orbital.Networking.NamedPipes
{
	public class NamedPipeConnection : INetworkDataSender, IDisposable
	{
		public delegate void DataRecievedCallbackMethod(NamedPipeConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(NamedPipeConnection connection, string message);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		private PipeStream nativePipe;
		public readonly NamedPipe pipe;
		public readonly string name;
		private bool isConnected;

		internal const int receiveBufferSize = 1024;
		private readonly byte[] receiveBuffer;
		private byte[] sendBuffer;

		public NamedPipeConnection(NamedPipe pipe, PipeStream nativePipe, string name)
		{
			this.pipe = pipe;
			this.nativePipe = nativePipe;
			this.name = name;

			receiveBuffer = new byte[receiveBufferSize];
			isConnected = true;
		}

		public void Dispose()
		{
			Dispose(null);
		}

		public void Dispose(string message)
		{
			bool wasConnected;
			lock (this)
			{
				wasConnected = isConnected;
				isConnected = false;

				if (nativePipe != null)
				{
					nativePipe.Dispose();
					nativePipe = null;
				}
			}

			pipe.RemoveConnection(this);
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

		internal void InitRecieve()
		{
			lock (this)
			{
				if (!nativePipe.IsConnected) throw new Exception("Pipe not connected for recieving data");
				nativePipe.BeginRead(receiveBuffer, 0, receiveBuffer.Length, RecieveDataCallback, null);
			}
		}

		protected void RecieveDataCallback(IAsyncResult ar)
		{
			int bytesRead = 0;
			bool disconnected = false;
			lock (this)
			{
				if (!isConnected) return;

				// handle failed reads
				try
				{
					bytesRead = nativePipe.EndRead(ar);
				}
				catch
				{
					disconnected = true;
					return;
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
						nativePipe.BeginRead(receiveBuffer, 0, receiveBuffer.Length, RecieveDataCallback, null);
					}
					catch
					{
						disconnected = true;
					}
				}
			}

			if (disconnected || !IsConnected()) Dispose("Disconnected");
		}

		public bool IsConnected()
		{
			lock (this) return isConnected && nativePipe != null && nativePipe.IsConnected;
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
				nativePipe.Write(sendBuffer, 0, size);
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
				nativePipe.Write(data, offset, size);
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
	}
}
