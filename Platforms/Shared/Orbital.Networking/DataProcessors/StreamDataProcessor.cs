using System;
using System.IO;

namespace Orbital.Networking.DataProcessors
{
	public class StreamDataProcessor : DataProcessor
	{
		public delegate void FinishedCallbackMethod(bool success);
		public event FinishedCallbackMethod FinishedCallback;

		private Stream stream;
		private long offset;
        private readonly long size;
		private bool done;
		
		/// <param name="stream">Stream to write data to</param>
		/// <param name="size">Final size stream is expected to be</param>
		public StreamDataProcessor(Stream stream, long size)
		{
			this.stream = stream;
			this.size = size;
		}

		/// <summary>
		/// Process incomming data stream
		/// </summary>
		/// <param name="data">Data of stream</param>
		/// <param name="offset">Offset into data</param>
		/// <param name="size">Size in Data object to read</param>
		public void Process(byte[] data, int offset, int size)
		{
			if (done) throw new Exception("Cant call 'Process' after 'FinishedCallback' has fired");

			stream.Write(data, offset, size);
			offset += size;
			if (offset == this.size)
			{
				done = true;
				FinishedCallback?.Invoke(true);
			}
			else if (offset > this.size)
			{
				done = true;
				FinishedCallback?.Invoke(false);
			}
		}
	}
}
