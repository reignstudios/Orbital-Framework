using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Orbital.Networking.Sockets.NetworkDiscovery
{
	public enum P2PBroadcasterEndPoint
	{
		ExternalOnly,
		LoopbackOnly,
		Both
	}

    public class P2PBroadcaster : P2P
    {
		private System.Timers.Timer timer;
		private byte[] metaData;
		private byte[] transmissionData;
		private P2PBroadcasterEndPoint broadcastEndPoint;
		private IPEndPoint externalEndPoint, loopbackEndPoint;

		public P2PBroadcaster(string id, IPAddress localEndPoint, int port, P2PBroadcasterEndPoint broadcastEndPoint, byte[] metaData = null)
		: base(id, port, metaData)
		{
			this.metaData = metaData;
			this.broadcastEndPoint = broadcastEndPoint;
			transmissionData = BuildTransmitionData(binaryID, metaData);

			udp = new UdpClient(AddressFamily.InterNetwork);
			udp.EnableBroadcast = true;
			udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			var endpoint = new IPEndPoint(localEndPoint, port);
			udp.Client.Bind(endpoint);
			externalEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
			loopbackEndPoint = new IPEndPoint(IPAddress.Parse("127.255.255.255"), port);
		}

		public void UpdateMetaData(byte[] metaData)
		{
			lock (this) transmissionData = BuildTransmitionData(binaryID, metaData);
		}

		public override void Dispose()
		{
			disposed = true;
			lock (this)
			{
				Stop();
				base.Dispose();
			}
		}

		public void Stop()
		{
			lock (this)
			{
				if (timer != null)
				{
					timer.Stop();
					timer.Dispose();
					timer = null;
				}
			}
		}

		private void CheckDisposed()
		{
			if (disposed) throw new Exception("Cannot broadcast: Object disposed");
		}

		public void Broadcast(int interval = 3000)
		{
			CheckDisposed();
			if (disconnected) return;

			lock (this)
			{
				// stop timer
				Stop();

				// start new broadcast timer
				timer = new System.Timers.Timer(interval);
				timer.Elapsed += Timer_Elapsed;
				timer.Enabled = true;
			}
		}

		public void ManualBroadcast(byte[] metaDataOverride = null)
		{
			CheckDisposed();
			if (disconnected) return;

			lock (this)
			{
				// stop timer
				if (metaDataOverride == null) Stop();

				// broadcast data
				if (metaDataOverride != null)
				{
					var transmissionDataOverride = BuildTransmitionData(binaryID, metaDataOverride);
					BroadcastData(transmissionDataOverride);
				
				}
				else
				{
					BroadcastData(transmissionData);
				}
			}
		}

		private void BroadcastData(byte[] data)
		{
			Exception e = null;
			lock (this)
			{
				try
				{
					if (udp != null)
					{
						if (broadcastEndPoint == P2PBroadcasterEndPoint.ExternalOnly || broadcastEndPoint == P2PBroadcasterEndPoint.Both) udp.Send(data, data.Length, externalEndPoint);
						if (broadcastEndPoint == P2PBroadcasterEndPoint.LoopbackOnly || broadcastEndPoint == P2PBroadcasterEndPoint.Both) udp.Send(data, data.Length, loopbackEndPoint);
					}
				}
				catch (Exception ex)
				{
					e = ex;
					disconnected = true;
				}
			}

			if (e != null) FireDisconnectedCallback(this, e);
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			BroadcastData(transmissionData);
		}
    }
}
