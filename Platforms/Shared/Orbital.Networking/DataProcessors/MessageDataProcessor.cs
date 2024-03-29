﻿using System;
using System.Runtime.InteropServices;

namespace Orbital.Networking.DataProcessors
{
	public class MessageDataProcessor : DataProcessor
	{
		public delegate void MessageRecievedCallbackMethod(byte[] messageData);
		public event MessageRecievedCallbackMethod MessageRecievedCallback;
		
		private int messageDataOffset, messageSizeDataOffset;
		private byte[] messageData, messageSizeData;
		private int messageSize;

		public MessageDataProcessor()
		{
			messageData = new byte[0];
			messageSizeData = new byte[sizeof(int)];
		}

		private static void ShiftBufferDown(byte[] data, int offset)
		{
			for (int i = 0; i != data.Length; ++i)
			{
				int i2 = i + offset;
				if (i2 >= 0 && i2 < data.Length) data[i] = data[i2];
				else data[i] = 0;
			}
		}

		private static void ShiftBufferUp(byte[] data, int offset)
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
		/// <param name="size">Size in Data object to read</param>
		public void Process(byte[] data, int size)
		{
			// check if this is a new message
			int messageSizeRead = 0;
			if (messageSizeDataOffset < sizeof(int))
			{
				// load message size
				int dstOffset = messageSizeDataOffset;
				messageSizeDataOffset += Math.Min(sizeof(int), size) - messageSizeDataOffset;
				int dstSize = messageSizeDataOffset - dstOffset;
				Array.Copy(data, 0, messageSizeData, dstOffset, dstSize);
				if (messageSizeDataOffset != sizeof(int)) return;
				else messageSize = BitConverter.ToInt32(messageSizeData, 0);
				
				// resize message buffer
				Array.Resize(ref messageData, messageSize);
				messageSizeRead = dstSize;
			}

			// calculate remaining data to read
			int dataRemainder = size - messageSizeRead;
			int dataRead = Math.Min(dataRemainder, messageSize);
			dataRemainder -= dataRead;

			// copy message data
			Array.Copy(data, messageSizeRead, messageData, messageDataOffset, dataRead);
			messageDataOffset += dataRead;

			// check if message finished
			if (messageDataOffset >= messageSize)
			{
				// fire message event
				MessageRecievedCallback?.Invoke(messageData);

				// reset
				messageDataOffset = 0;
				messageSizeDataOffset = 0;

				// process remainder
				if (dataRemainder != 0)
				{
					ShiftBufferDown(data, size - dataRemainder);
					Process(data, dataRemainder);
				}
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
