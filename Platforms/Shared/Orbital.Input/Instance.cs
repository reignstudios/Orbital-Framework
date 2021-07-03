using Orbital.Primitives;
using System;
using System.Collections.Generic;

namespace Orbital.Input
{
	public abstract class InstanceBase : IDisposable
	{
		/// <summary>
		/// Called when device connects
		/// </summary>
		public event ConnectedCallbackMethod DeviceConnectedCallback;
		public delegate void ConnectedCallbackMethod(DeviceBase device);

		/// <summary>
		/// Called when Gamepad connects
		/// </summary>
		public event GamepadConnectedCallbackMethod GamepadConnectedCallback;
		public delegate void GamepadConnectedCallbackMethod(Gamepad gamepad);

		/// <summary>
		/// Add devices this instance has access to
		/// </summary>
		public ReadOnlyArray<DeviceBase> devices { get; protected set; }

		/// <summary>
		/// Gamepad hardware configurations used to auto create 'gamepads'
		/// </summary>
		public GamepadHardwareConfiguration[] gamepadHardwareConfigurations;

		/// <summary>
		/// Gamepads configured from hardware configurations
		/// </summary>
		public ReadOnlyList<Gamepad> gamepads { get; private set; }
		private List<Gamepad> gamepads_backing;

		public InstanceBase()
		{
			gamepads = new ReadOnlyList<Gamepad>(out gamepads_backing);
		}

		public virtual void Dispose()
		{
			if (devices != null)
			{
				foreach (var device in devices) device.Dispose();
				devices = null;
			}
		}

		/// <summary>
		/// Update devices states
		/// </summary>
		public virtual void Update()
		{
			foreach (var device in devices)
			{
				bool wasConnected = device.connected;
				device.Update();
				if (wasConnected != device.connected)
				{
					if (device.connected)
					{
						// fire device connected callback
						if (DeviceConnectedCallback != null) DeviceConnectedCallback(device);

						// check if gamepad is mapped
						if (gamepadHardwareConfigurations != null)
						{
							int index = Gamepad.FindHardwareConfiguration(device, gamepadHardwareConfigurations);
							if (index >= 0)
							{
								var gamepad = new Gamepad(device);
								gamepad.Configure(gamepadHardwareConfigurations[index].config);
								gamepads_backing.Add(gamepad);
								if (GamepadConnectedCallback != null) GamepadConnectedCallback(gamepad);
							}
						}
					}
					else
					{
						// check if gamepad needs to be un-mapped
						if (gamepadHardwareConfigurations != null)
						{
							for (int i = 0; i != gamepads_backing.Count; ++i)
							{
								var gamepad = gamepads_backing[i];
								if (gamepad.device == device)
								{
									gamepad.Dispose();
									gamepads_backing.Remove(gamepad);
									break;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets built in / well-known configurations
		/// </summary>
		/// <returns>Common hardware configurations</returns>
		public virtual GamepadHardwareConfiguration[] GetGamepadHardwareConfigurations()
		{
			return null;
		}
	}
}
