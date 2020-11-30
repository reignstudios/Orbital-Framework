using System;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;
using HRESULT = System.Int32;
using HINSTANCE = System.IntPtr;

namespace Orbital.Input.DirectInput
{
	public sealed class Device : DeviceBase
	{
		public Instance instance { get; private set; }
		
		/// <summary>
		/// 8 controllers max
		/// </summary>
		public Controller[] controllers { get; private set; }

		public Device(Instance instance)
		{
			this.instance = instance;
			controllers = new Controller[8];
			for (int i = 0; i != controllers.Length; ++i) controllers[i] = new Controller(i);
		}

		public unsafe bool Init()
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
				case InstanceVersion.DI_8: Update_8(); break;
			}
		}

		private unsafe void Update_8()
		{
			for (uint i = 0; i != controllers.Length; ++i)
			{
				//XINPUT_STATE state;
				//bool connected = XInputGetState_1_3(i, &state) == 0;
				//controllers[i].Update(connected, ref state);
			}
		}
	}
}
