using System.Net;

namespace Orbital.Networking.Sockets
{
	public class RUDPSocketServer : RUDPSocket
	{
		public RUDPSocketServer(IPAddress remoteAddress, IPAddress localAddress, int port, int receiveBufferSize) : base(remoteAddress, localAddress, port, receiveBufferSize)
		{
		}
	}
}
