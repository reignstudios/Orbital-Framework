using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Orbital.Input.XInput
{
	public sealed class Device : DeviceBase
	{
		public Instance instance { get; private set; }
		
		/// <summary>
		/// 8 controllers max
		/// </summary>
		public Controller[] controllers { get; private set; }

		[DllImport(Instance.lib_1_3, CallingConvention = Instance.callingConvention)]
		private unsafe static extern DWORD XInputGetState_1_3(DWORD dwUserIndex, XINPUT_STATE* pState);

		public Device(Instance instance)
		{
			this.instance = instance;
			controllers = new Controller[8];
		}

		public bool Init()
		{
			return true;
		}

		public override void Dispose()
		{
			// do nothing...
		}

		public override void Update()
		{
			switch (instance.version)
			{
				case InstanceVersion.XInput_1_3: Update_1_3(); break;
			}
		}

		private unsafe void Update_1_3()
		{
			for (uint i = 0; i != controllers.Length; ++i)
			{
				XINPUT_STATE state;
				bool connected = XInputGetState_1_3(i, &state) == 0;
				controllers[i].Update(connected, ref state);
			}
		}
	}
}
