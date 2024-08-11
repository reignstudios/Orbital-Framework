using System;
using System.Collections.Generic;
using System.Text;

namespace Orbital.Networking
{
	public unsafe interface INetworkDataSender
	{
		void Send(byte* data, int size);
		void Send(byte* data, int offset, int size);
		void Send(byte[] data);
		void Send(byte[] data, int size);
		void Send(byte[] data, int offset, int size);
		void Send<T>(T data) where T : unmanaged;
		void Send<T>(T* data) where T : unmanaged;
		void Send(string text, Encoding encoding);
	}
}
