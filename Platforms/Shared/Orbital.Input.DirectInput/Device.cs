using System;
using System.Runtime.InteropServices;

namespace Orbital.Input.DirectInput
{
	public sealed class Device : DeviceBase
	{
		internal IntPtr handle;
		public Instance instanceDI { get; private set; }

		/// <summary>
		/// 8 controllers max
		/// </summary>
		public Controller[] controllers { get; private set; }

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_DirectInput_Device_Create();

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern int Orbital_Video_DirectInput_Device_Init(IntPtr handle, IntPtr instance, IntPtr window);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_DirectInput_Device_Dispose(IntPtr handle);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private unsafe static extern int Orbital_Video_DirectInput_Device_Update(IntPtr handle, int controllerIndex, DIJOYSTATE2* state, int* connected);

		public Device(Instance instance)
		: base(instance)
		{
			instanceDI = instance;
			controllers = new Controller[8];
			for (int i = 0; i != controllers.Length; ++i) controllers[i] = new Controller(i);
			handle = Orbital_Video_DirectInput_Device_Create();
		}

		public bool Init()// TODO: allow custom window so its handle can be used
		{
			if (Orbital_Video_DirectInput_Device_Init(handle, instanceDI.handle, IntPtr.Zero) == 0) return false;
			return true;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_DirectInput_Device_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public unsafe override void Update()
		{
			for (int i = 0; i != controllers.Length; ++i)
			{
				DIJOYSTATE2 state;
				int connected;
				if (Orbital_Video_DirectInput_Device_Update(handle, i, &state, &connected) == 0) connected = 0;
				controllers[i].Update(connected != 0, ref state);
			}
		}
	}
}
