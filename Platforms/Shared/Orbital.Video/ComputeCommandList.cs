using System;
using Orbital.Numerics;

namespace Orbital.Video
{
	public abstract class ComputeCommandListBase : CommandListBase
	{
		public readonly DeviceBase device;

		public ComputeCommandListBase(DeviceBase device)
		{
			this.device = device;
		}

		/// <summary>
		/// Sets compute state
		/// </summary>
		public abstract void SetComputeState(ComputeStateBase computeState);

		/// <summary>
		/// Executes compute shader last set with "SetComputeState"
		/// </summary>
		public abstract void ExecuteComputeShader(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ);
	}
}
