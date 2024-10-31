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
			int read = 0, chunkRead;

			// check if this is a new message
			if (messageSize < 0)
			{
				// read message size
				chunkRead = Math.Min(sizeof(int) - messageDataRead, size);
				Array.Copy(data, offset, messageData, messageDataRead, chunkRead);
				messageDataRead += chunkRead;
				offset += chunkRead;
				read += chunkRead;
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
			chunkRead = Math.Min(messageSize - messageDataRead, size);
			Array.Copy(data, offset, messageData, messageDataRead, chunkRead);
			messageDataRead += chunkRead;
			offset += chunkRead;
			read += chunkRead;

			// check if message finished
			if (messageDataRead >= messageSize)
			{
				// check if something is wrong. just kill the message stream
				if (messageDataRead > messageSize)
				{
					string error = string.Format("MessageDataProcessor read past message size messageDataRead:{0} messageSize:{1}", messageDataRead, messageSize);
					Console.WriteLine(error);
					System.Diagnostics.Debug.WriteLine(error);
					return;
				}

				// invoke message event
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
			size -= read;
			if (size > 0)
			{
				Process(data, offset, size);
			}
		}

		/// <summary>
		/// Prefixes the message data with its size
		/// </summary>
		public static unsafe void PrefixMessageData(ref byte[] messageData)
		{
			Array.Resize<byte>(ref messageData, messageData.Length + sizeof(int));
			ShiftBufferUp(messageData, sizeof(int));
			fixed (byte* messageDataPtr = messageData) *(int*)messageDataPtr = messageData.Length;
		}

		/// <summary>
		/// Prefixes the message data with its size into another buffer of adequate size
		/// </summary>
		public static unsafe void PrefixMessageData(byte[] messageData, int messageDataOffset, int messageDataSize, byte[] messageDataWithPrefix, int messageDataWithPrefixOffset)
		{
			int bufferSize = messageDataOffset + messageDataSize;
			if (messageData.Length < bufferSize) throw new Exception("messageData is to small");
			if (messageDataWithPrefix.Length + sizeof(int) < bufferSize) throw new Exception("messageDataWithPrefix is to small");
			fixed (byte* messageDataWithPrefixPtr = messageDataWithPrefix) *(int*)(messageDataWithPrefixPtr + messageDataWithPrefixOffset) = messageDataSize;// set message size
			Array.Copy(messageData, messageDataOffset, messageDataWithPrefix, messageDataWithPrefixOffset + sizeof(int), messageDataSize);// copy packet data
		}
	}
}
