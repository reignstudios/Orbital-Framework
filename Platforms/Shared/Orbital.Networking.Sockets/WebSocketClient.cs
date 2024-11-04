using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Orbital.Networking.Sockets
{
    public sealed class WebSocketClient : WebSocket
    {
        public WebSocketConnection connection {get; private set;}
		private ClientWebSocket nativeSocket;
		public readonly bool async;
		private readonly int timeout;

        /// <summary>
        /// WebSocketClient
        /// </summary>
        /// <param name="remoteAddress">Remote address we want to connect to</param>
		/// <param name="async">Auto handle receive events</param>
		/// <param name="timeout">Timeout in seconds (default no timeout)</param>
        public WebSocketClient(string remoteAddress, bool async, int timeout = -1)
		: base(remoteAddress)
		{
			this.async = async;
			this.timeout = timeout;
		}

		public override async void Dispose()
		{
			base.Dispose();

			// dispose native socket if connection object doesn't exist
			if (nativeSocket != null) await WebSocketConnection.Dispose(nativeSocket);
			nativeSocket = null;
		}

        /// <summary>
        /// Start attempting a connection
        /// </summary>
        public async void Connect()
		{
			await ConnectSync();
		}

		/// <summary>
        /// Start attempting a connection
        /// </summary>
        /// <returns>Connection if not in async mode, otherwise null</returns>
        public async Task<WebSocketConnection> ConnectSync()
		{
			if (isDisposed || nativeSocket != null) throw new Exception("Can only be called once");

			nativeSocket = new ClientWebSocket();
			using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout)))
			{
				try
				{
					await nativeSocket.ConnectAsync(new Uri(address), tokenSource.Token);
				}
				catch (Exception e)
				{
					InvokeConnectedCallback(this, null, false, e.Message);
					return null;
				}
            }

			if (isDisposed) return null;
            var connection = new WebSocketConnection(this, nativeSocket, address, async);
            _connections.Add(connection);
            connection.Init();
			InvokeConnectedCallback(this, connection, true, null);
			return connection;
		}

		internal override void RemoveConnection(WebSocketConnection connection)
		{
			lock (this)
			{
				base.RemoveConnection(connection);
				this.connection = null;
				nativeSocket = null;
			}
		}
    }
}