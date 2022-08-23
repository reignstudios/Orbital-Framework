using System;
using System.Collections.Generic;
using System.Text;

namespace Orbital.OS.Serial
{
	public class SerialDeviceConnectDesc
	{
		/// <summary>
		/// Vender ID
		/// </summary>
		public readonly ushort vid;

		/// <summary>
		/// Product ID
		/// </summary>
		public readonly ushort pid;

		/// <summary>
		/// Baud Rate (default is 9600)
		/// </summary>
		public int baudRate;

		/// <summary>
		/// Construct serial device
		/// </summary>
		/// <param name="vid">VID of device</param>
		/// <param name="pid">PID of device</param>
		public SerialDeviceConnectDesc(ushort vid, ushort pid)
		{
			this.vid = vid;
			this.pid = pid;
			this.baudRate = 9600;// default baud-rate
		}

		/// <summary>
		/// Construct serial device
		/// </summary>
		/// <param name="vid">VID of device</param>
		/// <param name="pid">PID of device</param>
		public SerialDeviceConnectDesc(ushort vid, ushort pid, int baudRate)
		{
			this.vid = vid;
			this.pid = pid;
			this.baudRate = baudRate;
		}
	}
}
