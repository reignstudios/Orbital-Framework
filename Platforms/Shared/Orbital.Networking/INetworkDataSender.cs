using System;
using System.Collections.Generic;
using System.Text;

namespace Orbital.Networking
{
	public interface INetworkDataSender
	{
		int Send(byte[] buffer);
		int Send(byte[] buffer, int offset, int size);
		int Send(string text, Encoding encoding);
	}
}
