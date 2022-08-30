using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace Orbital.Networking.NamedPipes
{
	public class NamedPipeConnection : INetworkDataSender, IDisposable
	{
		public delegate void DataRecievedCallbackMethod(NamedPipeConnection connection, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(NamedPipeConnection connection);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		private PipeStream nativePipe;
		public readonly NamedPipe pipe;
		public readonly string name;
		private bool isConnected;

		internal const int receiveBufferSize = 1024;
		private readonly byte[] receiveBuffer;

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
			if (wasConnected) DisconnectedCallback?.Invoke(this);
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
			if (!disconnected) DataRecievedCallback?.Invoke(this, receiveBuffer, bytesRead);

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

			if (disconnected || !IsConnected()) Dispose();
		}

		public bool IsConnected()
		{
			lock (this) return isConnected && nativePipe != null && nativePipe.IsConnected;
		}

		public int Send(byte[] buffer)
		{
			try
			{
				int size = buffer.Length;
				nativePipe.Write(buffer, 0, size);
				return size;
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
				nativePipe.Write(buffer, offset, size);
				return size;
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
			return Send(data);
		}
	}
}
