using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Orbital.Networking.Sockets.NetworkDiscovery
{
    public class P2PListener : P2P
    {
		public delegate void BroadcasterFoundCallbackMethod(IPEndPoint endpoint, byte[] metaData);
		public event BroadcasterFoundCallbackMethod BroadcasterFoundCallback;

		private IPEndPoint endpoint;
		private IPAddress localEndPoint;

		public P2PListener(string id, IPAddress localEndPoint, int port)
		: base(id, port, null)
		{
			this.localEndPoint = localEndPoint;
		}

		public void Listen()
		{
			if (disposed) throw new Exception("Cannot listen: Object disposed");

			lock (this)
			{
				if (udp != null) throw new Exception("Must call stop first");

				udp = new UdpClient(AddressFamily.InterNetwork);
				udp.EnableBroadcast = true;
				udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				endpoint = new IPEndPoint(localEndPoint, port);
				udp.Client.Bind(endpoint);

				// start UDP recieve listen
				udp.BeginReceive(RecieveCallback, null);
			}
		}

		public void Stop()
		{
			lock (this)
			{
				if (udp != null)
				{
					udp.Close();
					udp.Dispose();
					udp = null;
				}
			}
		}

		public override void Dispose()
		{
			lock (this)
			{
				base.Dispose();
				BroadcasterFoundCallback = null;
			}
		}

		private void RecieveCallback(IAsyncResult ar)
		{
			byte[] remoteMetaData = null;
			IPEndPoint remoteEndPoint = null;
			Exception e = null;
			lock (this)
			{
				if (udp == null) return;

				try
				{
					remoteEndPoint = new IPEndPoint(0, 0);
					var data = udp.EndReceive(ar, ref remoteEndPoint);
					if (data == null || data.Length < binaryID.Length) return;
					string remoteID = Encoding.ASCII.GetString(data, 0, binaryID.Length);
					if (remoteID == id)
					{
						int metaDataLength = data.Length - binaryID.Length;
						remoteMetaData = new byte[metaDataLength];
						Array.Copy(data, binaryID.Length, remoteMetaData, 0, metaDataLength);
						udp.BeginReceive(RecieveCallback, null);
					}
					else
					{
						udp.BeginReceive(RecieveCallback, null);
						return;
					}
				}
				catch (Exception ex)
				{
					e = ex;
					disconnected = true;
				}
			}

			if (e != null) FireDisconnectedCallback(this, e);
			else BroadcasterFoundCallback?.Invoke(IPAddress.IsLoopback(endpoint.Address) ? endpoint : remoteEndPoint, remoteMetaData);
		}
	}
}
