using System;
using System.Runtime.InteropServices;

namespace Orbital.Networking.DataProcessors
{
	public class MessageDataProcessor : DataProcessor
	{
		public delegate void MessageRecievedCallbackMethod(byte[] data, int size);
		public event MessageRecievedCallbackMethod MessageRecievedCallback;
		
		private int messageDataRead, messageSize;
		private byte[] messageData;

		public MessageDataProcessor()
		{
			messageSize = -1;
			messageData = new byte[sizeof(int)];// need at least enough space to read message size
		}

		public static void ShiftBufferDown(byte[] data, int offset)
		{
			for (int i = 0; i != data.Length; ++i)
			{
				int i2 = i + offset;
				if (i2 >= 0 && i2 < data.Length) data[i] = data[i2];
				else data[i] = 0;
			}
		}

		public static void ShiftBufferUp(byte[] data, int offset)
		{
			for (int i = (data.Length-1); i != -1; --i)
			{
				int i2 = i - offset;
				if (i2 >= 0 && i2 < data.Length) data[i] = data[i2];
				else data[i] = 0;
			}
		}

		/// <summary>
		/// Process incomming data stream
		/// </summary>
		/// <param name="data">Data of stream</param>
		/// <param name="offset">Offset into data</param>
		/// <param name="size">Size in Data object to read</param>
		public void Process(byte[] data, int offset, int size)
		{
			int read;

			// check if this is a new message
			if (messageSize < 0)
			{
				// read message size
				read = Math.Min(sizeof(int) - messageDataRead, size);
				Array.Copy(data, offset, messageData, messageDataRead, read);
				messageDataRead += read;
				offset += read;
				if (messageDataRead != sizeof(int))
				{
					return;// havent recieved message size yet
				}
				else
				{
					messageDataRead = 0;// reset as this offset should now match actual message size
					messageSize = BitConverter.ToInt32(messageData, 0);// we now have enough data to get message size
					if (messageData.Length < messageSize) Array.Resize(ref messageData, messageSize);// resize message buffer if needed
				}
			}

			// copy message data
			read = Math.Min(messageSize - messageDataRead, size - offset);
			Array.Copy(data, offset, messageData, messageDataRead, read);
			messageDataRead += read;
			offset += read;

			// check if message finished
			if (messageDataRead == messageSize)
			{
				// fire message event
				try
				{
					MessageRecievedCallback?.Invoke(messageData, messageSize);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					System.Diagnostics.Debug.WriteLine(e);
				}

				// reset
				messageDataRead = 0;
				messageSize = -1;
			}

			// process remainder
			if (offset < size)
			{
				Process(data, offset, size);
			}
		}

		/// <summary>
		/// Prefixes the message data with its size
		/// </summary>
		public static unsafe void PrefixMessageData(ref byte[] messageData)
		{
			int messageSize = messageData.Length;
			int prefixSize = Marshal.SizeOf<int>();
			Array.Resize<byte>(ref messageData, messageData.Length + prefixSize);
			ShiftBufferUp(messageData, prefixSize);
			fixed (byte* messageDataPtr = messageData) Buffer.MemoryCopy(&messageSize, messageDataPtr, prefixSize, prefixSize);
		}
	}
}
