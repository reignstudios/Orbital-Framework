using Orbital.Video;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace Orbital.Video.API.mGPU
{
	public sealed class VertexBufferStreamer : VertexBufferStreamerBase
	{
		public readonly Device deviceMGPU;
		public VertexBufferStreamerBase[] streamers { get; private set; }

		public VertexBufferStreamer(Device device, VertexBufferStreamerBase[] streamers)
		: base(device)
		{
			deviceMGPU = device;
			this.streamers = streamers;
		}

		public void Init(VertexBufferStreamLayout layout)
		{
			InitBase(ref layout);
		}

		public override void Dispose()
		{
			if (streamers != null)
			{
				foreach (var streamer in streamers)
				{
					if (streamer != null) streamer.Dispose();
				}
				streamers = null;
			}
		}
	}
}
