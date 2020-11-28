using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Orbital.Input;
using Orbital.Input.XInput;

namespace Orbital.Input.Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var instance = new Instance())
			using (var device = new Device(instance))
			{
				while (true)
				{
					device.Update();
					foreach (var controller in device.controllers)
					{
						if (!controller.connected) continue;

						// exit
						if (controller.menu.up) return;

						// rumble
						controller.SetRumble(controller.triggerLeft.value, controller.triggerRight.value);

						// buttons
						foreach (var button in controller.buttons)
						{
							if (button.down) Console.WriteLine(button.name);
						}

						// analogs 1D
						foreach (var analog in controller.analogs_1D)
						{
							if (analog.value > 0) Console.WriteLine(analog.name + " " + analog.value.ToString());
						}

						// analogs 2D
						foreach (var analog in controller.analogs_2D)
						{
							if (analog.value.Length() > 0) Console.WriteLine(analog.name + " " + analog.value.ToString());
						}
					}
					System.Threading.Thread.Sleep(1);
				}
			}
		}
	}
}
