using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Orbital.Networking.DataProcessors;
using Orbital.Networking.Sockets;

namespace Orbital.Demo.Networking
{
	internal class Program
	{
		static void Main(string[] args)
		{
			// is server?
			Console.WriteLine("Is Server? (y/n)");
			string result = Console.ReadLine();
			if (string.IsNullOrEmpty(result) || result != "y" && result != "n")
			{
				Console.WriteLine("Invalid argument");
				Console.ReadLine();
				return;
			}
			bool isServer = result == "y";

			// get local address
			Console.WriteLine("Enter your IP Address...");
			result = Console.ReadLine();
			IPAddress localAddress = null;
			if (string.IsNullOrEmpty(result))
			{
				Console.WriteLine("Invalid ip");
				Console.ReadLine();
				return;
			}

			if (!IPAddress.TryParse(result, out localAddress))
			{
				Console.WriteLine("Invalid local address");
				Console.ReadLine();
				return;
			}

			// get server address
			IPAddress serverAddress = null;
			if (!isServer)
			{
				Console.WriteLine("Enter server IP Address...");
				result = Console.ReadLine();
				if (string.IsNullOrEmpty(result))
				{
					Console.WriteLine("Invalid ip");
					Console.ReadLine();
					return;
				}

				if (!IPAddress.TryParse(result, out serverAddress))
				{
					Console.WriteLine("Invalid server address");
					Console.ReadLine();
					return;
				}
			}

			// connect
			var socket = new RUDPSocket(IPAddress.Any, localAddress, 8080, 1024);
			socket.ListenDisconnectedErrorCallback += Socket_ListenDisconnectedErrorCallback;
			socket.ConnectedCallback += Socket_ConnectedCallback;
			socket.Listen(1);
			if (!isServer) socket.Connect(serverAddress);

			// messaging
			Console.WriteLine("Type Messages after you have a connection (or 'q' to quit)...");
			string message = null;
			while (true)
			{
				message = Console.ReadLine();
				if (message == "q") break;
				foreach (var connection in socket.connections)
				{
					connection.Send(message, Encoding.ASCII);
				}
			}

			// shutdown
			socket.Dispose();
		}

		private static void Socket_ListenDisconnectedErrorCallback(RUDPSocket sender, string message)
		{
			Console.WriteLine("ERROR: " + message);
		}

		private static void Socket_ConnectedCallback(RUDPSocket sender, RUDPSocketConnection connection, bool success, string message)
		{
			Console.WriteLine("Connected: " + connection.address.ToString());
			connection.DataRecievedCallback += Connection_DataRecievedCallback;
			connection.DisconnectedCallback += Connection_DisconnectedCallback;
		}

		private static void Connection_DataRecievedCallback(RUDPSocketConnection connection, byte[] data, int offset, int size)
		{
			string message = Encoding.ASCII.GetString(data, offset, size);
			Console.WriteLine(string.Format("Message From:({0}) {1}", connection.address, message));
		}

		private static void Connection_DisconnectedCallback(RUDPSocketConnection connection)
		{
			Console.WriteLine("Diconnected: " + connection.address.ToString());
		}
	}
}
