namespace Orbital.Video
{
	public enum DeviceType
	{
		/// <summary>
		/// Device will be used for presenting rendered buffers on a physical screen
		/// </summary>
		Presentation,

		/// <summary>
		/// Device will only be used for background processing (such as Compute-Shaders, etc)
		/// </summary>
		Background
	}

	public abstract class DeviceBase
	{
		public readonly DeviceType type;

		public DeviceBase(DeviceType type)
		{
			this.type = type;
		}

		/// <summary>
		/// Do any prep work needed before new presentation frame
		/// </summary>
		public abstract void BeginFrame();

		/// <summary>
		/// Finish and present frame to physical screen
		/// </summary>
		public abstract void EndFrame();
	}
}
