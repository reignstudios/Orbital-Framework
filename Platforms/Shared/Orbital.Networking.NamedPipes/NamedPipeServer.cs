using System;
using System.IO.Pipes;

namespace Orbital.Networking.NamedPipes
{
	public class NamedPipeServer : NamedPipe
    {
		private NamedPipeServerStream listenPipe;
		private bool isListening;
		private int maxConnections;

		public delegate void ListenStoppedErrorCallbackMethod(NamedPipeServer server, string message);
		public event ListenStoppedErrorCallbackMethod ListenDisconnectedErrorCallback;

		public NamedPipeServer(string name, PipeDirection direction, PipeOptions options)
		: base(name, direction, options)
		{}

		public override void Dispose()
		{
			isDisposed = true;

			lock (this)
			{
				isListening = false;

				if (listenPipe != null)
				{
					listenPipe.Dispose();
					listenPipe = null;
				}
			}

			base.Dispose();
			ListenDisconnectedErrorCallback = null;
		}

		public bool IsListening()
		{
			lock (this) return !isDisposed && isListening;
		}

		/// <summary>
		/// Start listening for connections
		/// </summary>
		/// <param name="maxConnections">Max number of allowed connections</param>
		public void Listen(int maxConnections)
		{
			lock (this)
			{
				if (isDisposed || listenPipe != null) throw new Exception("Can only be called once");

				this.maxConnections = maxConnections;
				listenPipe = new NamedPipeServerStream
				(
					name, direction, maxConnections,
					PipeTransmissionMode.Byte,
					options,
					NamedPipeConnection.receiveBufferSize, NamedPipeConnection.receiveBufferSize
				);

				isListening = true;
				try
				{
					listenPipe.BeginWaitForConnection(WaitForConnectionCallback, listenPipe);
				}
				catch (Exception e)
				{
					isListening = false;
					throw e;
				}
			}
		}

		private void WaitForConnectionCallback(IAsyncResult ar)
		{
			var serverPipe = (NamedPipeServerStream)ar.AsyncState;
			NamedPipeConnection connection = null;
			bool success = false;
			string message = null;
			
			// connect
			lock (this)
			{
				if (isDisposed) return;
				
				try
				{
					serverPipe.EndWaitForConnection(ar);
					connection = new NamedPipeConnection(this, serverPipe, name);
					_connections.Add(connection);
					success = true;
					listenPipe = null;
				}
				catch (Exception e)
				{
					success = false;
					message = "Failed to EndWaitForConnection: " + e.Message;
				}
			}

			// fire connected callback
			FireConnectedCallback(this, connection, success, message);

			// start recieving data
			bool disconnected = false;
			lock (this)
			{
				if (success && !isDisposed && (direction == PipeDirection.InOut || direction == PipeDirection.In))
				{
					try
					{
						connection.InitRecieve();
					}
					catch
					{
						disconnected = true;
					}
				}
			}

			if (disconnected) connection.Dispose();

			// listen for next connection
			TryListen();
		}

		private void TryListen()
		{
			string listenError = null;
			lock (this)
			{
				if (!isDisposed && isListening && _connections.Count < maxConnections)
				{
					try
					{
						listenPipe = new NamedPipeServerStream
						(
							name, direction, maxConnections,
							PipeTransmissionMode.Byte,
							options,
							NamedPipeConnection.receiveBufferSize, NamedPipeConnection.receiveBufferSize
						);

						listenPipe.BeginWaitForConnection(WaitForConnectionCallback, listenPipe);
					}
					catch (Exception e)
					{
						isListening = false;
						listenError = e.Message;
					}
				}
			}

			if (listenError != null)
			{
				try
				{
					ListenDisconnectedErrorCallback?.Invoke(this, listenError);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					System.Diagnostics.Debug.WriteLine(e);
				}
			}
		}

		internal override void RemoveConnection(NamedPipeConnection connection)
		{
			lock (this)
			{
				base.RemoveConnection(connection);
				TryListen();
			}
		}
	}
}
