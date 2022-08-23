using System;
using System.Collections.Generic;
using System.Text;

namespace Orbital.Networking.DataProcessors
{
    public interface DataProcessor
    {
		/// <summary>
		/// Process incomming data stream
		/// </summary>
		/// <param name="data">Data of stream</param>
		/// <param name="size">Size in Data object to read</param>
		void Process(byte[] data, int size);
    }
}
