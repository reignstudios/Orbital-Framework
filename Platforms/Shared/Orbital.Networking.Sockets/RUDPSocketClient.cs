using System.Net;

namespace Orbital.Networking.Sockets
{
	public class RUDPSocketClient : RUDPSocket
	{
		public RUDPSocketClient(IPAddress remoteAddress, IPAddress localAddress, int port, int receiveBufferSize) : base(remoteAddress, localAddress, port, receiveBufferSize)
		{
		}
	}
}
