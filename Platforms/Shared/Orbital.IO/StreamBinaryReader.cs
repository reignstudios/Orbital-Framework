using System;
using System.IO;

namespace Orbital.IO
{
	/// <summary>
	/// A fast portable alternative to BinaryReader that doesn't close the stream
	/// </summary>
	public class StreamBinaryReader
	{
		public readonly Stream stream;
		private byte[] buffer;

		public StreamBinaryReader(Stream stream)
		{
			this.stream = stream;
			buffer = new byte[8];
		}

		private void Read(int size)
		{
			int read = stream.Read(buffer, 0, size);
			if (read < size) throw new Exception("End of file reached");
		}

		public char ReadChar()
		{
			Read(sizeof(char));
			return BitConverter.ToChar(buffer, 0);
		}

		public short ReadInt16()
		{
			Read(sizeof(short));
			return BitConverter.ToInt16(buffer, 0);
		}

		public int ReadInt32()
		{
			Read(sizeof(int));
			return BitConverter.ToInt32(buffer, 0);
		}

		public long ReadInt64()
		{
			Read(sizeof(long));
			return BitConverter.ToInt64(buffer, 0);
		}

		public ushort ReadUInt16()
		{
			Read(sizeof(ushort));
			return BitConverter.ToUInt16(buffer, 0);
		}

		public uint ReadUInt32()
		{
			Read(sizeof(uint));
			return BitConverter.ToUInt32(buffer, 0);
		}

		public ulong ReadUInt64()
		{
			Read(sizeof(ulong));
			return BitConverter.ToUInt64(buffer, 0);
		}

		public float ReadSingle()
		{
			Read(sizeof(float));
			return BitConverter.ToSingle(buffer, 0);
		}

		public double ReadDouble()
		{
			Read(sizeof(double));
			return BitConverter.ToDouble(buffer, 0);
		}
	}
}
