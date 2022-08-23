using Orbital.Numerics;
using Orbital.Primitives;
using System;

namespace Orbital.Input.API
{
	public sealed class Device : DeviceBase
	{
		public Instance instanceAPI { get; private set; }
		internal DeviceBase device;

		public Device(Instance instance)
		: base(instance)
		{
			instanceAPI = instance;
			type = DeviceType.Gamepad;
			CreatePhysicalObjects(0, 0, 0, 0, 0, 0);
		}

		public unsafe override void Update()
		{
			// get device state
			bool connected = device != null && device.connected;

			// validate is connected
			UpdateStart(connected);
			if (!connected) return;

			// copy states
			device.buttons.Copy(buttons_backing);
			device.axes1D.Copy(axes1D_backing);
			device.axes2D.Copy(axes2D_backing);
			device.axes3D.Copy(axes3D_backing);
			device.sliders.Copy(sliders_backing);
			device.povDirections.Copy(povDirections_backing);
		}

		protected unsafe override void RefreshDeviceInfo()
		{
			// do nothing (handled in device abstraction)
		}

		public unsafe override void SetRumble(float value)
		{
			if (device != null) device.SetRumble(value);
		}

		public unsafe override void SetRumble(float leftValue, float rightValue)
		{
			if (device != null) device.SetRumble(leftValue, rightValue);
		}

		public unsafe override void SetRumble(float value, int motorIndex)
		{
			if (device != null) device.SetRumble(value, motorIndex);
		}
	}
}
