using System;
using System.Net;

namespace Orbital.Networking.Sockets
{
    public abstract class Socket : IDisposable
    {
		public readonly IPAddress address;
		public readonly Guid addressID;
		public readonly int port;
		public readonly IPEndPoint endPoint;
		protected bool isDisposed;

		public Socket(IPAddress address, int port)
		{
			this.address = address;
			addressID = AddressToAddressID(address);
			this.port = port;
			endPoint = new IPEndPoint(address, port);
		}

		public virtual void Dispose()
		{
			isDisposed = true;
		}

		public abstract bool IsConnected();
		
		public unsafe static IPAddress AddressIDToAddress(Guid addressID)
		{
			var bytes = addressID.ToByteArray();
			bool isIPV6 = false;
			for (int i = 4; i < 16; ++i)
			{
				if (bytes[i] != 0)
				{
					isIPV6 = true;
					break;
				}
			}

			if (!isIPV6)
			{
				var newBytes = new byte[4];
				Array.Copy(bytes, newBytes, newBytes.Length);
				bytes = newBytes;
			}

			return new IPAddress(bytes);
		}

		public static Guid AddressToAddressID(IPAddress address)
		{
			var aadressBytes = address.GetAddressBytes();
			if (aadressBytes.Length < 16)
			{
				var newBytes = new byte[16];
				Array.Copy(aadressBytes, newBytes, aadressBytes.Length);
				aadressBytes = newBytes;
			}
			return new Guid(aadressBytes);
		}
    }
}
