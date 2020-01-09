using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Orbital.Host;
using Orbital.Numerics;
using Orbital.Video;
using Orbital.Video.API;

namespace Orbital.Demo
{
	[StructLayout(LayoutKind.Sequential)]
	struct Vertex
	{
		public Vec3 position;
		public Color4 color;
		public Vertex(Vec3 position, Color4 color)
		{
			this.position = position;
			this.color = color;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct ConstantBufferObject
	{
		public float offset;
		public float constrast;
	}

	public sealed class Example : IDisposable
	{
		private ApplicationBase application;
		private WindowBase window;

		private InstanceBase instance;
		private DeviceBase device;
		private CommandListBase commandList;
		private RenderPassBase renderPass;
		private RenderStateBase renderState;
		private ShaderEffectBase shaderEffect;
		private VertexBufferBase vertexBuffer;
		private ConstantBufferBase constantBuffer;

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

			// load api abstraction (api-instance and hardware-device)
			var abstractionDesc = new AbstractionDesc(true);
			abstractionDesc.supportedAPIs = new AbstractionAPI[] {AbstractionAPI.D3D12};

			abstractionDesc.deviceDescD3D12.window = window;
			abstractionDesc.nativeLibPathD3D12 = Path.Combine(platformPath, @"Shared\Orbital.Video.D3D12.Native\bin", libFolderBit, config);

			abstractionDesc.deviceDescVulkan.window = window;
			abstractionDesc.nativeLibPathVulkan = Path.Combine(platformPath, @"Shared\Orbital.Video.Vulkan.Native\bin", libFolderBit, config);
			
			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out instance, out device)) throw new Exception("Failed to init abstraction");

			// create command list
			commandList = device.CreateCommandList();

			// create render pass
			var renderPassDesc = new RenderPassDesc()
			{
				clearColor = true,
				clearColorValue = new Vec4(0, .2f, .4f, 1)
			};
			renderPass = device.CreateRenderPass(renderPassDesc);

			// create constant buffer
			var constantBufferObject = new ConstantBufferObject()
			{
				offset = .5f,
				constrast = .5f
			};
			constantBuffer = device.CreateConstantBuffer<ConstantBufferObject>(constantBufferObject, ConstantBufferMode.Static);

			// load shaders
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
				desc.constantBuffers = new ShaderEffectConstantBuffer[1];
				desc.constantBuffers[0] = new ShaderEffectConstantBuffer()
				{
					registerIndex = 0,
					size = constantBuffer.size
				};
				shaderEffect = device.CreateShaderEffect(vs, ps, null, null, null, desc, true);
			}

			// create vertex buffer
			var vertexBufferLayout = new VertexBufferLayout();
			vertexBufferLayout.elements = new VertexBufferLayoutElement[2];
			vertexBufferLayout.elements[0] = new VertexBufferLayoutElement()
			{
				type = VertexBufferLayoutElementType.Float3,
				usage = VertexBufferLayoutElementUsage.Position,
				streamIndex = 0, usageIndex = 0, byteOffset = 0
			};
			vertexBufferLayout.elements[1] = new VertexBufferLayoutElement()
			{
				type = VertexBufferLayoutElementType.RGBAx8,
				usage = VertexBufferLayoutElementUsage.Color,
				streamIndex = 0, usageIndex = 0, byteOffset = (sizeof(float) * 3)
			};
			
			var vertices = new Vertex[]
			{
				new Vertex(new Vec3(-1, -1, 0), Color4.red),
				new Vertex(new Vec3(0, 1, 0), Color4.green),
				new Vertex(new Vec3(1, -1, 0), Color4.blue)
			};
			vertexBuffer = device.CreateVertexBuffer<Vertex>(vertices, vertexBufferLayout, VertexBufferMode.Static);

			// create render state
			var renderStateDesc = new RenderStateDesc()
			{
				renderPass = renderPass,
				shaderEffect = shaderEffect,
				vertexBuffer = vertexBuffer,
				vertexBufferTopology = VertexBufferTopology.Triangle
			};
			renderState = device.CreateRenderState(renderStateDesc, 0);

			// print all GPUs this abstraction supports
			if (!instance.QuerySupportedAdapters(false, out var adapters)) throw new Exception("Failed: QuerySupportedAdapters");
			foreach (var adapter in adapters) Debug.WriteLine(adapter.name);
		}

		public void Dispose()
		{
			if (constantBuffer != null)
			{
				constantBuffer.Dispose();
				constantBuffer = null;
			}

			if (vertexBuffer != null)
			{
				vertexBuffer.Dispose();
				vertexBuffer = null;
			}

			if (renderState != null)
			{
				renderState.Dispose();
				renderState = null;
			}

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
			application.RunEvents();// run this once before checking window
			while (!window.IsClosed())
			{
				device.BeginFrame();
				commandList.Start();
				commandList.BeginRenderPass(renderPass);
				var windowSize = window.GetSize(WindowSizeType.WorkingArea);
				commandList.SetViewPort(new ViewPort(new Rect2(0, 0, windowSize.width, windowSize.height)));
				commandList.SetRenderState(renderState);
				commandList.SetConstantBuffer(constantBuffer, 0);
				commandList.Draw();
				commandList.EndRenderPass();
				commandList.Finish();
				commandList.Execute();
				device.EndFrame();

				// run application events
				application.RunEvents();
			}
		}
	}
}
