using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Orbital.Networking.DataProcessors;
using Orbital.Networking.Sockets;
using Orbital.Networking;

namespace Orbital.Demo.Networking
{
	internal class Program
	{
		private static MessageDataProcessor messageProcessor;

		static void Main(string[] args)
		{
			// use TCP?
			Console.WriteLine("Use TCP? (y/n)");
			string result = Console.ReadLine();
			if (string.IsNullOrEmpty(result) || result != "y" && result != "n")
			{
				Console.WriteLine("Invalid argument");
				Console.ReadLine();
				return;
			}
			bool useTCP = result == "y";

			// is server?
			Console.WriteLine("Is Server? (y/n)");
			result = Console.ReadLine();
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

			// create message processor
			messageProcessor = new MessageDataProcessor();
			messageProcessor.MessageRecievedCallback += MessageProcessor_MessageRecievedCallback;

			// run messaging demo
			if (useTCP)
			{
				// connect
				if (isServer)
				{
					using (var tcpSocketServer = new TCPSocketServer(localAddress, 8080, true, true, timeout:60))
					{
						tcpSocketServer.ListenDisconnectedErrorCallback += TCPSocket_ListenDisconnectedErrorCallback;
						tcpSocketServer.ConnectedCallback += TCPSocket_ConnectedCallback;
						tcpSocketServer.Listen(16);

						// messaging
						Console.WriteLine("Type Messages after you have a connection (or 'q' to quit)...");
						string message = null;
						while (true)
						{
							message = Console.ReadLine();
							if (message == "q") break;
							foreach (var connection in tcpSocketServer.connections)
							{
								SendMessage(connection, message);
							}
						}
					}
				}
				else
				{
					using (var tcpSocketClient = new TCPSocketClient(serverAddress, localAddress, 8080, true, true, timeout:60))
					{
						tcpSocketClient.ConnectedCallback += TCPSocket_ConnectedCallback;
						tcpSocketClient.Connect();

						// messaging
						Console.WriteLine("Type Messages after you have a connection (or 'q' to quit)...");
						string message = null;
						while (true)
						{
							message = Console.ReadLine();
							if (message == "q") break;
							foreach (var connection in tcpSocketClient.connections)
							{
								SendMessage(connection, message);
							}
						}
					}
				}
			}
			else
			{
				// connect
				using (var rudpSocket = new RUDPSocket(IPAddress.Any, localAddress, 8080, true, 1024, RUDPMode.Stream))
				{
					rudpSocket.ListenDisconnectedErrorCallback += RUDPSocket_ListenDisconnectedErrorCallback;
					rudpSocket.ConnectedCallback += RUDPSocket_ConnectedCallback;
					rudpSocket.Listen(16);
					if (!isServer) rudpSocket.Connect(serverAddress);

					// messaging
					Console.WriteLine("Type Messages after you have a connection (or 'q' to quit)...");
					string message = null;
					while (true)
					{
						message = Console.ReadLine();
						if (message == "q") break;
						foreach (var connection in rudpSocket.connections)
						{
							SendMessage(connection, message);
						}
					}
				}
			}
		}

		private static void SendMessage(INetworkDataSender sender, string message)
		{
			var data = Encoding.ASCII.GetBytes(message);
			MessageDataProcessor.PrefixMessageData(ref data);
			sender.Send(data);
		}

		// =======================
		// Message Processor
		// =======================
		private static void MessageProcessor_MessageRecievedCallback(byte[] data, int size)
		{
			string message = Encoding.ASCII.GetString(data, 0, size);
			Console.WriteLine(string.Format("Message: '{0}'", message));
		}

		// =======================
		// TCP
		// =======================
		private static void TCPSocket_ListenDisconnectedErrorCallback(TCPSocketServer sender, string message)
		{
			Console.WriteLine("ERROR: " + message);
		}

		private static void TCPSocket_ConnectedCallback(TCPSocket socket, TCPSocketConnection connection, bool success, string message)
		{
			Console.WriteLine("Connected: " + connection.address.ToString());
			connection.DataRecievedCallback += TCPConnection_DataRecievedCallback;
			connection.DisconnectedCallback += TCPConnection_DisconnectedCallback;
		}

		private static void TCPConnection_DataRecievedCallback(TCPSocketConnection connection, byte[] data, int size)
		{
			Console.WriteLine(string.Format("Data From:({0}) Size:{1}", connection.address, size));
			messageProcessor.Process(data, 0, size);
		}

		private static void TCPConnection_DisconnectedCallback(TCPSocketConnection connection, string message)
		{
			Console.WriteLine(string.Format("Diconnected: {0} '{1}'", connection.address, message));
		}

		// =======================
		// RUDP
		// =======================
		private static void RUDPSocket_ListenDisconnectedErrorCallback(RUDPSocket sender, string message)
		{
			Console.WriteLine("ERROR: " + message);
		}

		private static void RUDPSocket_ConnectedCallback(RUDPSocket sender, RUDPSocketConnection connection, bool success, string message)
		{
			Console.WriteLine("Connected: " + connection.address.ToString());
			connection.DataRecievedCallback += RUDPConnection_DataRecievedCallback;
			connection.DisconnectedCallback += RUDPConnection_DisconnectedCallback;
		}

		private static void RUDPConnection_DataRecievedCallback(RUDPSocketConnection connection, byte[] data, int offset, int size)
		{
			Console.WriteLine(string.Format("Data From:({0}) Size:{1}", connection.address, size));
			messageProcessor.Process(data, offset, size);
		}

		private static void RUDPConnection_DisconnectedCallback(RUDPSocketConnection connection, string message)
		{
			Console.WriteLine(string.Format("Diconnected: {0} '{1}'", connection.address, message));
		}
	}
}
