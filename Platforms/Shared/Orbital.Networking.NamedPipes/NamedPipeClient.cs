using System;
using System.IO.Pipes;
using System.Threading;

namespace Orbital.Networking.NamedPipes
{
	public class NamedPipeClient : NamedPipe
    {
		public NamedPipeConnection connection { get; private set; }
		private NamedPipeClientStream nativePipe;

		private CancellationTokenSource cancelToken;

		public NamedPipeClient(string name, PipeDirection direction, PipeOptions options)
		: base(name, direction, options)
		{}

		public override void Dispose()
		{
			isDisposed = true;

			NamedPipeConnection connectionObj;
			lock (this)
			{
				if (cancelToken != null)
				{
					if (!cancelToken.IsCancellationRequested) cancelToken.Cancel();
					cancelToken = null;
				}

				if (connection == null && nativePipe != null) nativePipe.Dispose();
				nativePipe = null;

				connectionObj = connection;
				connection = null;
			}

			if (connectionObj != null) connectionObj.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// Start attempting a connection
		/// </summary>
		/// <param name="timeoutMS">Time before connection attempt is canceled. -1 for infinite wait</param>
		public void Connect(int timeoutMS = 5000)
		{
			lock (this)
			{
				if (isDisposed || nativePipe != null) throw new Exception("Can only be called once");
				nativePipe = new NamedPipeClientStream(".", name, direction, options);
				cancelToken = new CancellationTokenSource();
				ConnectAsync(nativePipe, timeoutMS);
			}
		}

		private async void ConnectAsync(NamedPipeClientStream clientPipe, int timeout)
		{
			bool success = false;
			string message = null;

			// connect
			try
			{
				if (timeout == -1) await clientPipe.ConnectAsync(cancelToken.Token);
				else await clientPipe.ConnectAsync(timeout, cancelToken.Token);
				if (clientPipe.IsConnected)
				{
					connection = new NamedPipeConnection(this, clientPipe, name);
					connection = connection;
					_connections.Add(connection);
					success = true;
				}
			}
			catch (Exception e)
			{
				success = false;
				message = "Failed to ConnectAsync: " + e.Message;
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

			if (disconnected) Dispose();
		}

		internal override void RemoveConnection(NamedPipeConnection connection)
		{
			base.RemoveConnection(connection);
			lock (this)
			{
				if (!isDisposed && this.connection != connection) throw new Exception("Connection objects don't match (this should never happen)");
				this.connection = null;
				this.nativePipe = null;
			}
		}
	}
}
