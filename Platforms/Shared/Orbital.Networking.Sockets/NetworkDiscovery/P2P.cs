using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Orbital.Networking.Sockets.NetworkDiscovery
{
    public abstract class P2P : IDisposable
    {
		public readonly string id;
        public readonly int port;
        protected readonly byte[] binaryID;
        protected UdpClient udp;
		protected bool disposed, disconnected;

        public P2P(string id, int port, byte[] metaData)
        {
			this.id = id;
            this.port = port;
			binaryID = Encoding.ASCII.GetBytes(id);
        }

		protected static byte[] BuildTransmitionData(byte[] binaryID, byte[] metaData)
		{
			var transmissionData = new byte[binaryID.Length + (metaData != null ? metaData.Length : 0)];
			Array.Copy(binaryID, transmissionData, binaryID.Length);
			if (metaData != null) Array.Copy(metaData, 0, transmissionData, binaryID.Length, metaData.Length);
			return transmissionData;
		}

        public virtual void Dispose()
        {
			disposed = true;
			lock (this)
			{
				if (udp != null)
				{
					udp.Close();
					udp.Dispose();
					udp = null;
				}
			}

			if (!disconnected) DisconnectedCallback?.Invoke(this, null);
			disconnected = true;
			DisconnectedCallback = null;
        }

		public delegate void DisconnectedCallbackMethod(P2P p2p, Exception e);
		public event DisconnectedCallbackMethod DisconnectedCallback;
		protected void FireDisconnectedCallback(P2P p2p, Exception e)
		{
			disconnected = true;
			DisconnectedCallback?.Invoke(p2p, e);
		}
	}
}
