using System;
using System.Runtime.InteropServices;

namespace Orbital.Video.D3D12
{
	public sealed class ShaderEffect : ShaderEffectBase
	{
		public readonly Device deviceD3D12;
		internal IntPtr handle;

		public Shader vs {get; private set;}
		public Shader ps {get; private set;}
		public Shader hs {get; private set;}
		public Shader ds {get; private set;}
		public Shader gs {get; private set;}
		private bool disposeShaders;

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern IntPtr Orbital_Video_D3D12_ShaderEffect_Create(IntPtr device);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static unsafe extern int Orbital_Video_D3D12_ShaderEffect_Init(IntPtr handle, IntPtr vs, IntPtr ps, IntPtr hs, IntPtr ds, IntPtr gs, ShaderEffectDesc_NativeInterop* desc);

		[DllImport(Instance.lib, CallingConvention = Instance.callingConvention)]
		private static extern void Orbital_Video_D3D12_ShaderEffect_Dispose(IntPtr handle);

		public ShaderEffect(Device device)
		: base(device)
		{
			deviceD3D12 = device;
			handle = Orbital_Video_D3D12_ShaderEffect_Create(device.handle);
		}

		public bool Init(Shader vs, Shader ps, Shader hs, Shader ds, Shader gs, ShaderEffectDesc desc, bool disposeShaders)
		{
			this.vs = vs;
			this.ps = ps;
			this.hs = hs;
			this.ds = ds;
			this.gs = gs;
			this.disposeShaders = disposeShaders;
			return InitFinish(ref desc);
		}

		protected unsafe override bool InitFinish(ref ShaderEffectDesc desc)
		{
			if (desc.constantBuffers != null) constantBufferCount = desc.constantBuffers.Length;
			if (desc.textures != null) textureCount = desc.textures.Length;

			IntPtr vsHandle = vs != null ? vs.handle : IntPtr.Zero;
			IntPtr psHandle = ps != null ? ps.handle : IntPtr.Zero;
			IntPtr hsHandle = hs != null ? hs.handle : IntPtr.Zero;
			IntPtr dsHandle = ds != null ? ds.handle : IntPtr.Zero;
			IntPtr gsHandle = gs != null ? gs.handle : IntPtr.Zero;
			using (var nativeDesc = new ShaderEffectDesc_NativeInterop(ref desc))
			{
				return Orbital_Video_D3D12_ShaderEffect_Init(handle, vsHandle, psHandle, hsHandle, dsHandle, gsHandle, &nativeDesc) != 0;
			}
		}

		protected override bool CreateShader(byte[] data, ShaderType type)
		{
			switch (type)
			{
				case ShaderType.VS:
					vs = new Shader(deviceD3D12, type);
					return vs.Init(data);

				case ShaderType.PS:
					ps = new Shader(deviceD3D12, type);
					return ps.Init(data);

				default: throw new NotSupportedException("Shader type not supported: " + type.ToString());
			}
		}

		public override void Dispose()
		{
			if (handle != IntPtr.Zero)
			{
				Orbital_Video_D3D12_ShaderEffect_Dispose(handle);
				handle = IntPtr.Zero;
			}

			if (!disposeShaders)
			{
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
}
