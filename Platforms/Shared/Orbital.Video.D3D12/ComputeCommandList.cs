using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class ComputeCommandList : ComputeCommandListBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		internal ComputeCommandList(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = CommandList.Orbital_Video_D3D12_CommandList_Create(device.handle);
		}

		public bool Init()
		{
			return CommandList.Orbital_Video_D3D12_CommandList_Init(handle, CommandListType.Compute) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				CommandList.Orbital_Video_D3D12_CommandList_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Start(int nodeIndex)
		{
			CommandList.Orbital_Video_D3D12_CommandList_Start(handle, nodeIndex);
		}

		public override void Finish()
		{
			CommandList.Orbital_Video_D3D12_CommandList_Finish(handle);
		}

		public override void SetComputeState(ComputeStateBase computeState)
		{
			var computeStateD3D12 = (ComputeState)computeState;
			CommandList.Orbital_Video_D3D12_CommandList_SetComputeState(handle, computeStateD3D12.handle);
		}

		public override void ExecuteComputeShader(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
		{
			CommandList.Orbital_Video_D3D12_CommandList_ExecuteComputeShader(handle, (uint)threadGroupCountX, (uint)threadGroupCountY, (uint)threadGroupCountZ);
		}

		public override void Execute()
		{
			CommandList.Orbital_Video_D3D12_CommandList_Execute(handle);
		}
	}
}
