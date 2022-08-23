using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Orbital.OS.Serial
{
	public enum SerialType
	{
		Unknown,
		COM
	}

	public class SerialDeviceDesc
	{
		public readonly string friendlyName, portName, hardwareID;
		public readonly ushort vid, pid;
		public readonly int portNumber = -1;
		public readonly SerialType portType = SerialType.Unknown;

		public SerialDeviceDesc(string friendlyName, string portName, string hardwareID)
		{
			this.friendlyName = friendlyName;
			this.portName = portName;
			this.hardwareID = hardwareID;

			if (portName.StartsWith("COM") && int.TryParse(portName.Substring("COM".Length), out portNumber))
			{
				portType = SerialType.COM;
			}
			else
			{
				portNumber = -1;
			}

			var rx = Regex.Match(hardwareID, @"VID_(\w*)&PID_(\w*)", RegexOptions.IgnoreCase);
			if (rx.Success)
			{
				vid = ushort.Parse(rx.Groups[1].Value, NumberStyles.HexNumber);
				pid = ushort.Parse(rx.Groups[2].Value, NumberStyles.HexNumber);
			}
		}
	}
}
