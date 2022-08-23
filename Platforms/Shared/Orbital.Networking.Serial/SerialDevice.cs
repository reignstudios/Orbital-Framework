using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Orbital.OS.Serial;

namespace Orbital.Networking.Serial
{
	/// <summary>
	/// Serial device disconnection error type
	/// </summary>
	public enum DisconnectionError
	{
		/// <summary>
		/// Normal shutdown
		/// </summary>
		None,

		/// <summary>
		/// Common disconnect
		/// </summary>
		Disconnected,

		/// <summary>
		/// Failed to write data to serial device
		/// </summary>
		WriteFailed,

		/// <summary>
		/// Failed to read data from serial device
		/// </summary>
		ReadFailed,

		/// <summary>
		/// State change in the serial connection
		/// </summary>
		PinChanged,

		/// <summary>
		/// Error in serial connection
		/// </summary>
		ErrorRecieved
	}

	public class SerialDevice : IDisposable, INetworkDataSender
	{
		public delegate void DataRecievedCallbackMethod(SerialDevice serialDevice, byte[] data, int size);
		public event DataRecievedCallbackMethod DataRecievedCallback;

		public delegate void DisconnectedCallbackMethod(SerialDevice serialDevice, ushort vid, ushort pid, string frendlyName, int portNumber, DisconnectionError error, string errorMessage);
		public event DisconnectedCallbackMethod DisconnectedCallback;

		private SerialPort serial;
		private readonly bool serialOwner = true;
		private bool isDisposed, connected;
		private byte[] recieveData, writeSingleByteBuffer = new byte[1];
		public readonly int readTimeout = -1, writeTimeout = -1;

		/// <summary>
		/// Name of device currently connected
		/// </summary>
		public string connectedFrendlyName { get; private set; }

		/// <summary>
		/// Port number currently connected to
		/// </summary>
		public int connectPortNumber { get; private set; }

		/// <summary>
		/// VID currently connected to
		/// </summary>
		public ushort connectedVID { get; private set; }

		/// <summary>
		/// PID currently connected to
		/// </summary>
		public ushort connectedPID { get; private set; }

		/// <summary>
		/// Constructs with existing serial connection. If port not open, an open will be attempted.
		/// </summary>
		/// <param name="serialPort">Existing serial port</param>
		/// <param name="owner">Do we own the 'serialPort' obj? If we don't own it, close and dispose will not be called</param>
		public SerialDevice(SerialPort serialPort, bool owner, int receiveBufferSize)
		{
			this.serial = serialPort;
			serialOwner = owner;
			recieveData = new byte[receiveBufferSize];
			connectPortNumber = -1;
			serialPort.DataReceived += SerialPort_DataReceived;
			serialPort.ErrorReceived += SerialPort_ErrorReceived;
			serialPort.PinChanged += SerialPort_PinChanged;
		}

		public SerialDevice(int receiveBufferSize)
		{
			recieveData = new byte[receiveBufferSize];
		}

		/// <summary>
		/// Connects to first avaliale device
		/// </summary>
		public void Connect(List<SerialDeviceConnectDesc> devices, bool DtrEnable = false, bool RtsEnable = false)
		{
			lock (this)
			{
				if (isDisposed || serial != null) throw new Exception("Can only be called once or can't be called if external serial-port was used in constructor");

				// find first server serial port that connects
				var serialPorts = SerialUtils.GetSerialPortDevices(devices);
				foreach (var port in serialPorts)
				{
					SerialPort trySerialPort = null;
					try
					{
						var device = devices.Find(x => x.vid == port.vid && x.pid == port.pid);
						trySerialPort = new SerialPort(port.portName, device.baudRate);
						trySerialPort.Encoding = Encoding.ASCII;
						trySerialPort.DtrEnable = DtrEnable;
						trySerialPort.RtsEnable = RtsEnable;
						trySerialPort.ReadTimeout = 2000;
						trySerialPort.WriteTimeout = 2000;
						if (readTimeout > 0) trySerialPort.ReadTimeout = readTimeout;
						if (writeTimeout > 0) trySerialPort.WriteTimeout = writeTimeout;

						trySerialPort.DataReceived += SerialPort_DataReceived;
						trySerialPort.ErrorReceived += SerialPort_ErrorReceived;
						trySerialPort.PinChanged += SerialPort_PinChanged;

						trySerialPort.Open();
						serial = trySerialPort;
						connectedFrendlyName = port.friendlyName;
						connectPortNumber = port.portNumber;
						connectedVID = port.vid;
						connectedPID = port.pid;
						connected = true;
						break;
					}
					catch
					{
						if (trySerialPort != null)
						{
							trySerialPort.Dispose();
							trySerialPort = null;
						}
					}
				}

				if (serial == null) throw new Exception("Failed to connect to SerialPort");
			}
		}

		/// <summary>
		/// Connects device
		/// </summary>
		public void Connect(SerialDeviceConnectDesc device)
		{
			var devices = new List<SerialDeviceConnectDesc>() {device};
			Connect(devices);
		}

		/// <summary>
		/// Connects device
		/// </summary>
		public void Connect(ushort vid, ushort pid)
		{
			var devices = new List<SerialDeviceConnectDesc>() { new SerialDeviceConnectDesc(vid, pid) };
			Connect(devices);
		}

		/// <summary>
		/// Connects device
		/// </summary>
		public void Connect(ushort vid, ushort pid, int baudRate)
		{
			var devices = new List<SerialDeviceConnectDesc>() {new SerialDeviceConnectDesc(vid, pid, baudRate)};
			Connect(devices);
		}

		public void Dispose()
		{
			Dispose(DisconnectionError.None, null);
		}

		private void Dispose(DisconnectionError error, string errorMessage)
		{
			isDisposed = true;
			bool wasConnected;
			int portNumber;
			string frendlyName;
			ushort vid, pid;
			lock (this)
			{
				wasConnected = connected;
				portNumber = connectPortNumber;
				frendlyName = connectedFrendlyName;
				vid = connectedVID;
				pid = connectedPID;
				connected = false;
				connectPortNumber = -1;
				connectedFrendlyName = null;
				connectedVID = 0;
				connectedPID = 0;
				if (serial != null)
				{
					serial.DataReceived -= SerialPort_DataReceived;
					serial.ErrorReceived -= SerialPort_ErrorReceived;
					serial.PinChanged -= SerialPort_PinChanged;
					if (serialOwner)
					{
						try
						{
							if (serial.IsOpen) serial.Close();
						}
						catch { }

						try
						{
							serial.Dispose();
						}
						catch { }

						serial = null;
					}
				}
			}

			if (wasConnected) DisconnectedCallback?.Invoke(this, vid, pid, frendlyName, portNumber, error, errorMessage);
			DataRecievedCallback = null;
			DisconnectedCallback = null;
		}

		private bool IsConnected(bool canDispose)
		{
			lock (this)
			{
				 bool c = !isDisposed && connected && serial != null && serial.IsOpen;
				 if (canDispose && !c && connected) Dispose(DisconnectionError.Disconnected, "Disconnected");
				 return c;
			}
		}

		public bool IsConnected()
		{
			return IsConnected(true);
		}

		/// <summary>
		/// Read all existing data & discard
		/// </summary>
		public void FlushPending()
		{
			while (serial.BytesToRead > 0) serial.ReadByte();
		}

		public int Send(byte value)
		{
			Exception exception;
			lock (this)
			{
				try
				{
					writeSingleByteBuffer[0] = value;
					serial.Write(writeSingleByteBuffer, 0, 1);
					return 1;
				}
				catch (Exception e)
				{
					exception = e;
				}
			}

			if (!IsConnected(false)) Dispose(DisconnectionError.WriteFailed, exception.Message);
			throw exception;
		}

		public int Send(byte[] buffer)
		{
			Exception exception;
			lock (this)
			{
				try
				{
					int size = buffer.Length;
					serial.Write(buffer, 0, size);
					return size;
				}
				catch (Exception e)
				{
					exception = e;
				}
			}

			if (!IsConnected(false)) Dispose(DisconnectionError.WriteFailed, exception.Message);
			throw exception;
		}

		public int Send(byte[] buffer, int offset, int size)
		{
			Exception exception;
			lock (this)
			{
				try
				{
					serial.Write(buffer, offset, size);
					return size;
				}
				catch (Exception e)
				{
					exception = e;
				}
			}

			if (!IsConnected(false)) Dispose(DisconnectionError.WriteFailed, exception.Message);
			throw exception;
		}

		public void Send(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
			Send(data);
		}

		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			int read;
			while (true)
			{
				Exception exception = null;
				lock (this)
				{
					if (!IsConnected(false) || serial.BytesToRead <= 0) return;
					try
					{
						read = serial.Read(recieveData, 0, recieveData.Length);
						if (read <= 0) break;
					}
					catch (Exception ex)
					{
						exception = ex;
						break;
					}
				}

				if (exception == null)
				{
					DataRecievedCallback?.Invoke(this, recieveData, read);
				}
				else
				{
					Dispose(DisconnectionError.ReadFailed, exception.Message);
					return;
				}
			}
		}

		private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
		{
			Dispose(DisconnectionError.ErrorRecieved, e.EventType.ToString());
		}

		private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			Dispose(DisconnectionError.PinChanged, e.EventType.ToString());
		}
	}
}
