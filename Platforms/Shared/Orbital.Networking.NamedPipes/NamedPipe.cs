using System;
using System.IO.Pipes;
using System.IO;
using System.Collections.Generic;

namespace Orbital.Networking.NamedPipes
{
	public abstract class NamedPipe : IDisposable
	{
		protected List<NamedPipeConnection> _connections;
		public IReadOnlyList<NamedPipeConnection> connections { get { return _connections; } }

		public readonly string name;
		public readonly PipeDirection direction;
		public readonly PipeOptions options;
		protected bool isDisposed;

		public NamedPipe(string name, PipeDirection direction, PipeOptions options)
		{
			this.name = name;
			this.direction = direction;
			this.options = options;
			_connections = new List<NamedPipeConnection>();
		}

		public virtual void Dispose()
		{
			isDisposed = true;

			List<NamedPipeConnection> connectionsObj;
			lock (this)
			{
				connectionsObj = _connections;
				_connections = null;
			}

			if (connectionsObj != null)
			{
				for (int i = connectionsObj.Count - 1; i != -1; --i) connectionsObj[i].Dispose();
			}

			ConnectedCallback = null;
		}

		public bool IsConnected()
		{
			lock (this) return !isDisposed && _connections != null && _connections.Count != 0;
		}

		public delegate void ConnectedCallbackMethod(NamedPipe pipe, NamedPipeConnection connection, bool success, string message);
		public event ConnectedCallbackMethod ConnectedCallback;
		protected void FireConnectedCallback(NamedPipe pipe, NamedPipeConnection connection, bool success, string message)
		{
			ConnectedCallback?.Invoke(pipe, connection, success, message);
		}

		internal virtual void RemoveConnection(NamedPipeConnection connection)
		{
			lock (this)
			{
				if (_connections != null) _connections.Remove(connection);
			}
		}

		public static string[] FindAllPipes()
		{
			const string pipePrefix = @"\\.\pipe\";
			var pipes = Directory.GetFiles(pipePrefix);
			for (int i = 0; i != pipes.Length; ++i)
			{
				pipes[i] = pipes[i].Replace(pipePrefix, "");
			}
			return pipes;
		}
    }
}
