using Orbital.Primitives;
using System.Runtime.InteropServices;
using System.Text;

namespace Orbital.Input.API
{
	public sealed partial class Instance : InstanceBase
	{
		/// <summary>
		/// 16 devices max
		/// </summary>
		public ReadOnlyArray<Device> devicesAPI { get; private set; }

		private InstanceBase[] instances;
		private bool autoConfigureAbstractions;

		public Instance(bool autoConfigureAbstractions)
		: base(autoConfigureAbstractions)
		{
			this.autoConfigureAbstractions = autoConfigureAbstractions;

			Device[] devices_backing;
			devicesAPI = new ReadOnlyArray<Device>(16, out devices_backing);
			for (int i = 0; i != devices_backing.Length; ++i) devices_backing[i] = new Device(this);
			devices = new ReadOnlyArray<DeviceBase>(devices_backing);
		}

		public unsafe bool Init()
		{
			#if WIN
			bool ignoreXInputDevices = true;// TODO: only set true if XInput is used
			instances = new InstanceBase[2];
			instances[0] = new XInput.Instance(autoConfigureAbstractions);
			instances[1] = new DirectInput.Instance(ignoreXInputDevices, autoConfigureAbstractions);
			#endif
			return false;
		}

		public override void Update()
		{
			// TODO: configure devices based on vid/pid prefs
			base.Update();
		}
	}
}
