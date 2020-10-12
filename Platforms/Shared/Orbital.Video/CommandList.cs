using System;
using Orbital.Numerics;

namespace Orbital.Video
{
	public enum CommandListType
	{
		Rasterize,
		Compute
	}

	public abstract class CommandListBase : IDisposable
	{
		public abstract void Dispose();

		/// <summary>
		/// Start so we can record new commands (clears existing commands).
		/// NOTE: Always runs commands on primary GPU
		/// </summary>
		public void Start()
		{
			Start(0);
		}

		/// <summary>
		/// Start so we can record new commands (clears existing commands).
		/// NOTE: Runs commands on swap-chains active GPU. (In the case of AFR this will be next swap buffer device)
		/// </summary>
		/// <param name="swapChain">SwapChain to get active GPU node from</param>
		public void Start(SwapChainBase swapChain)
		{
			Start(swapChain.currentNodeIndex);
		}

		/// <summary>
		/// Start so we can record new commands (clears existing commands)
		/// </summary>
		/// <param name="nodeIndex">GPU node to record and execute commands</param>
		public abstract void Start(int nodeIndex);

		/// <summary>
		/// Finish so we can execute commands (no new commands can be added)
		/// </summary>
		public abstract void Finish();

		/// <summary>
		/// Executes command-list operations
		/// </summary>
		public abstract void Execute();
	}
}
