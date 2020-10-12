using Orbital.Numerics;
using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.Vulkan
{
	public sealed class ComputeCommandList : ComputeCommandListBase
	{
		public readonly Device deviceVulkan;
		internal IntPtr handle;

		internal ComputeCommandList(Device device)
		: base(device)
		{
			deviceVulkan = device;
			handle = CommandList.Orbital_Video_Vulkan_CommandList_Create(device.handle);
		}

		public bool Init()
		{
			return CommandList.Orbital_Video_Vulkan_CommandList_Init(handle, CommandListType.Compute) != 0;
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				CommandList.Orbital_Video_Vulkan_CommandList_Dispose(handle);
				handle = IntPtr.Zero;
			}
		}

		public override void Start(int nodeIndex)
		{
			CommandList.Orbital_Video_Vulkan_CommandList_Start(handle);
		}

		public override void Finish()
		{
			CommandList.Orbital_Video_Vulkan_CommandList_Finish(handle);
		}

		public override void SetComputeState(ComputeStateBase computeState)
		{
			throw new NotImplementedException();
		}

		public override void ExecuteComputeShader(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
		{
			throw new NotImplementedException();
		}

		public override void Execute()
		{
			CommandList.Orbital_Video_Vulkan_CommandList_Execute(handle);
		}
	}
}
