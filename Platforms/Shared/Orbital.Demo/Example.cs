using System;
using System.Diagnostics;
using System.IO;
using Orbital.Host;
using Orbital.Video;
using Orbital.Video.API;

namespace Orbital.Demo
{
	public sealed class Example : IDisposable
	{
		private ApplicationBase application;
		private WindowBase window;

		private InstanceBase instance;
		private DeviceBase device;
		private CommandListBase commandList;
		private RenderPassBase renderPass;
		private ShaderEffectBase shaderEffect;

		public Example(ApplicationBase application, WindowBase window)
		{
			this.application = application;
			this.window = window;
		}

		public void Init(string platformPath, string folder64Bit, string folder32Bit)
		{
			// pre-load native libs
			string libFolderBit;
			if (IntPtr.Size == 8) libFolderBit = folder64Bit;
			else if (IntPtr.Size == 4) libFolderBit = folder32Bit;
			else throw new NotSupportedException("Unsupported bit size: " + IntPtr.Size.ToString());

			#if RELEASE
			const string config = "Release";
			#else
			const string config = "Debug";
			#endif

			// load api abstraction
			var abstractionDesc = new AbstractionDesc(true);
			abstractionDesc.supportedAPIs = new AbstractionAPI[] {AbstractionAPI.D3D12};

			abstractionDesc.deviceDescD3D12.window = window;
			abstractionDesc.nativeLibPathD3D12 = Path.Combine(platformPath, @"Shared\Orbital.Video.D3D12.Native\bin", libFolderBit, config);

			abstractionDesc.deviceDescVulkan.window = window;
			abstractionDesc.nativeLibPathVulkan = Path.Combine(platformPath, @"Shared\Orbital.Video.Vulkan.Native\bin", libFolderBit, config);
			
			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out instance, out device)) throw new Exception("Failed to init abstraction");
			commandList = device.CreateCommandList();

			var renderPassDesc = new RenderPassDesc()
			{
				clearColor = true,
				clearColorValue = new Numerics.Vec4(1, 0, 0, 1)
			};
			renderPass = device.CreateRenderPass(renderPassDesc);

			// TODO: load CS2X compiled ShaderEffect
			/*using (var stream = new FileStream("Shader.se", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				shaderEffect = device.CreateShaderEffect(stream, ShaderEffectSamplerAnisotropy.Default);
			}*/

			using (var vsStream = new FileStream("Shaders\\Shader_D3D12.vs", FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var psStream = new FileStream("Shaders\\Shader_D3D12.ps", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var vs = new Video.D3D12.Shader((Video.D3D12.Device)device, ShaderType.VS);
				var ps = new Video.D3D12.Shader((Video.D3D12.Device)device, ShaderType.PS);
				if (!vs.Init(vsStream)) throw new Exception("Failed to init VS shader");
				if (!ps.Init(psStream)) throw new Exception("Failed to init PS shader");
				var desc = new ShaderEffectDesc();
				shaderEffect = device.CreateShaderEffect(vs, ps, null, null, null, desc, true);
			}

			// print all GPUs this abstraction supports
			if (!instance.QuerySupportedAdapters(false, out var adapters)) throw new Exception("Failed: QuerySupportedAdapters");
			foreach (var adapter in adapters) Debug.WriteLine(adapter.name);
		}

		public void Dispose()
		{
			if (shaderEffect != null)
			{
				shaderEffect.Dispose();
				shaderEffect = null;
			}

			if (renderPass != null)
			{
				renderPass.Dispose();
				renderPass = null;
			}

			if (commandList != null)
			{
				commandList.Dispose();
				commandList = null;
			}

			if (device != null)
			{
				device.Dispose();
				device = null;
			}

			if (instance != null)
			{
				instance.Dispose();
				instance = null;
			}
		}

		public void Run()
		{
			while (!window.IsClosed())
			{
				application.RunEvents();

				device.BeginFrame();
				commandList.Start();
				commandList.BeginRenderPass(renderPass);
				// TODO: draw stuff
				commandList.EndRenderPass(renderPass);
				commandList.Finish();
				commandList.Execute();
				device.EndFrame();
			}
		}
	}
}
