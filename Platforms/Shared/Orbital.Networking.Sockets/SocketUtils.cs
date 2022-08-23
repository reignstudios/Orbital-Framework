using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Orbital.Networking.Sockets
{
    public static class SocketUtils
    {
		public static int GetAddressAsInt(IPAddress address)
		{
			var binary = address.GetAddressBytes();
			return BitConverter.ToInt32(binary, 0);
		}

		public static int GetAddressAsInt(PhysicalAddress address)
		{
			var binary = address.GetAddressBytes();
			return BitConverter.ToInt32(binary, 0);
		}

		public static List<IPAddress> GetLocalIPAddresses()
		{
			var addresses = new List<IPAddress>();
			var entry = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var address in entry.AddressList)
			{
				if (address.AddressFamily == AddressFamily.InterNetwork) addresses.Add(address);
			}

			return addresses;
		}

		public static List<IPAddress> GetConnectedIPAddresses(params NetworkInterfaceType[] networkTypes)
		{
			var addresses = new List<IPAddress>();
			foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				// TODO: find network adapters that don't have a gateway (but are non-virtual only connection) for LAN only networks
				/*var gateway = networkInterface.GetIPProperties().GatewayAddresses.FirstOrDefault();
				if (gateway == null || gateway.Address == IPAddress.Any ||
					networkInterface.OperationalStatus != OperationalStatus.Up || !networkInterface.Supports(NetworkInterfaceComponent.IPv4)) continue;*/

				if (networkInterface.OperationalStatus != OperationalStatus.Up || !networkInterface.Supports(NetworkInterfaceComponent.IPv4)) continue;
				var ipProperties = networkInterface.GetIPProperties();
				var type = networkInterface.NetworkInterfaceType;
				if (networkTypes.Contains(type))
				{
					foreach (var ip in ipProperties.UnicastAddresses)
					{
						if (ip.Address != null && ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							addresses.Add(ip.Address);
						}
					}
				}
			}

			return addresses;
		}

		public static IPAddress ResolveHost(string host)
		{
			if (host == Dns.GetHostName()) return IPAddress.Loopback;

			var entry = Dns.GetHostEntry(host);
			foreach (var address in entry.AddressList)
			{
				if (address.AddressFamily == AddressFamily.InterNetwork) return address;
			}

			throw new Exception("No valid host found");
		}

		public static string ResolveHostFromIP(IPAddress ip)
		{
			var entry = Dns.GetHostEntry(ip);
			if (entry != null) return entry.HostName;
			return null;
		}

		public unsafe static PhysicalAddress GetMacAddress(IPAddress address)
		{
			if (address.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("GetMacAddress only supports IPv4 Addresses.");
			if (IPAddress.IsLoopback(address)) return PhysicalAddress.None;

			foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				var properties = networkInterface.GetIPProperties();
				if (properties == null) continue;
				foreach (var uniAddress in properties.UnicastAddresses)
				{
					if (uniAddress.Address.Equals(address))
					{
						var macAddress = networkInterface.GetPhysicalAddress();
						if (macAddress != null) return macAddress;
						else return PhysicalAddress.None;
					}
				}
			}

			return PhysicalAddress.None;
		}
	}
}
