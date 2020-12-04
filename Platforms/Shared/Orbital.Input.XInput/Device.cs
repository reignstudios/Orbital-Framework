using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Orbital.Input.XInput
{
	public sealed class Device : DeviceBase
	{
		public Instance instanceXI { get; private set; }
		
		/// <summary>
		/// 4 controllers max
		/// </summary>
		public Controller[] controllers { get; private set; }

		[DllImport(Instance.lib_1_3, CallingConvention = Instance.callingConvention, EntryPoint = "XInputGetState")]
		private unsafe static extern DWORD XInputGetState_1_3(DWORD dwUserIndex, XINPUT_STATE* pState);

		public Device(Instance instance)
		: base(instance)
		{
			instanceXI = instance;
			controllers = new Controller[4];
			for (int i = 0; i != controllers.Length; ++i) controllers[i] = new Controller(i);
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
			switch (instanceXI.version)
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
