using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Orbital.Input.XInput;
using System;

public class Demo_Input : MonoBehaviour
{
    private Instance instance;

	private void Start()
    {
        instance = new Instance();
        if (!instance.Init()) throw new Exception("ERROR: failed to init instance");
    }

	private void OnDestroy()
	{
		if (instance != null)
		{
			instance.Dispose();
			instance = null;
		}
	}

	private void Update()
    {
		instance.Update();
		foreach (var device in instance.devices)
		{
			if (!device.connected) continue;

			// rumble
			device.SetRumble(device.triggerLeft.value, device.triggerRight.value);

			// buttons
			foreach (var button in device.buttons)
			{
				if (button.down) Debug.Log(button.name);
			}

			// analogs 1D
			foreach (var analog in device.analogs_1D)
			{
				if (analog.value > 0) Debug.Log(analog.name + " " + analog.value.ToString());
			}

			// analogs 2D
			foreach (var analog in device.analogs_2D)
			{
				if (analog.value.Length() > 0) Debug.Log(analog.name + " " + analog.value.ToString());
			}
		}
	}
}
