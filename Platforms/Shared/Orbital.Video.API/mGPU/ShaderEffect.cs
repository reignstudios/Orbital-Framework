using System.Runtime.InteropServices;
using System;

namespace Orbital.Video.API.mGPU
{
	public sealed class ShaderEffect : ShaderEffectBase
	{
		public readonly Device deviceMGPU;
		public ShaderEffectBase[] effects { get; private set; }

		public ShaderEffect(Device device, ShaderEffectBase[] effects)
		: base(device)
		{
			deviceMGPU = device;
			this.effects = effects;
		}

		protected override bool CreateShader(byte[] data, ShaderType type)
		{
			throw new NotImplementedException();// this should never be called
		}

		public override void Dispose()
		{
			if (effects != null)
			{
				foreach (var effect in effects)
				{
					if (effect != null) effect.Dispose();
				}
				effects = null;
			}
		}
	}
}
