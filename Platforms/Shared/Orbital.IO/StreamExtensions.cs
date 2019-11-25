using System;
using System.IO;

namespace Orbital.IO
{
	public static class StreamExtensions
	{
		public static void ToByteArray(this Stream stream, out byte[] data, out int dataLength, int chunckSize)
		{
			data = new byte[chunckSize];
			int lastRead;
			dataLength = 0;
			do
			{
				int maxRead = data.Length - dataLength;
				lastRead = stream.Read(data, dataLength, maxRead);
				dataLength += lastRead;
				if (lastRead == maxRead)// increase buffer size in case next read contains more data
				{
					var newData = new byte[data.Length + chunckSize];
					Array.Copy(data, 0, newData, 0, dataLength);
					data = newData;
				}
			} while (lastRead != 0);
		}

		public static void ToByteArray(this Stream stream, out byte[] data, out int dataLength)
		{
			ToByteArray(stream, out data, out dataLength, 1024);
		}

		public static void ToByteArray(this Stream stream, out byte[] data)
		{
			ToByteArray(stream, out data, out int dataLength);
			if (data.Length != dataLength)// ensure data length matches read length
			{
				var newData = new byte[dataLength];
				Array.Copy(data, 0, newData, 0, dataLength);
				data = newData;
			}
		}
	}
}
