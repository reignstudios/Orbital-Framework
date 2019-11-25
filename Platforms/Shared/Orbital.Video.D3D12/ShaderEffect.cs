using System;

namespace Orbital.Video.D3D12
{
	public sealed class ShaderEffect : ShaderEffectBase
	{
		internal readonly Device device;

		public Shader vs {get; private set;}
		public Shader ps {get; private set;}
		public Shader hs {get; private set;}
		public Shader ds {get; private set;}
		public Shader gs {get; private set;}
		private bool disposeShaders;

		public ShaderEffect(Device device)
		{
			this.device = device;
		}

		public void Init(Shader vs, Shader ps, Shader hs, Shader ds, Shader gs, bool disposeShaders)
		{
			this.vs = vs;
			this.ps = ps;
			this.hs = hs;
			this.ds = ds;
			this.gs = gs;
			this.disposeShaders = disposeShaders;
		}

		protected override bool CreateShader(byte[] data, ShaderType type)
		{
			switch (type)
			{
				case ShaderType.VS:
					vs = new Shader(device, type);
					return vs.Init(data, ShaderDataType.CS2X);

				case ShaderType.PS:
					ps = new Shader(device, type);
					return ps.Init(data, ShaderDataType.CS2X);

				default: throw new NotSupportedException("Shader type not supported: " + type.ToString());
			}
		}

		public override void Dispose()
		{
			if (!disposeShaders) return;

			if (vs != null)
			{
				vs.Dispose();
				vs = null;
			}

			if (ps != null)
			{
				ps.Dispose();
				ps = null;
			}

			if (hs != null)
			{
				hs.Dispose();
				hs = null;
			}

			if (ds != null)
			{
				ds.Dispose();
				ds = null;
			}

			if (gs != null)
			{
				gs.Dispose();
				gs = null;
			}
		}
	}
}
