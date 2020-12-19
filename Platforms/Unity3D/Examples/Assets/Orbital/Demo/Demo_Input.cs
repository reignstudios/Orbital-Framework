using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Orbital.Input.XInput;
using System;

public class Demo_Input : MonoBehaviour
{
    private Instance instance;
    private Device device;

	private void Start()
    {
        instance = new Instance();
        if (!instance.Init()) throw new Exception("ERROR: failed to init instance");

        device = new Device(instance);
        if (!device.Init()) throw new Exception("ERROR: failed to init device");
    }

	private void OnDestroy()
	{
		if (device != null)
		{
			device.Dispose();
			device = null;
		}

		if (instance != null)
		{
			instance.Dispose();
			instance = null;
		}
	}

	private void Update()
    {
		device.Update();
		foreach (var controller in device.controllers)
		{
			if (!controller.connected) continue;

			// rumble
			controller.SetRumble(controller.triggerLeft.value, controller.triggerRight.value);

			// buttons
			foreach (var button in controller.buttons)
			{
				if (button.down) Debug.Log(button.name);
			}

			// analogs 1D
			foreach (var analog in controller.analogs_1D)
			{
				if (analog.value > 0) Debug.Log(analog.name + " " + analog.value.ToString());
			}

			// analogs 2D
			foreach (var analog in controller.analogs_2D)
			{
				if (analog.value.Length() > 0) Debug.Log(analog.name + " " + analog.value.ToString());
			}
		}
	}
}
