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
		public Vec2 uv;
		public Vertex(Vec3 position, Color4 color, Vec2 uv)
		{
			this.position = position;
			this.color = color;
			this.uv = uv;
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
		private IndexBufferBase indexBuffer;
		private ConstantBufferBase constantBuffer;
		private Texture2DBase texture, texture2;

		private ConstantBufferObject constantBufferObject;
		private float rot;

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

			// create texture
			int textureWidth = 256, textureHeight = 256;
			var textureData = new byte[textureWidth * textureHeight * 4];
			for (int y = 0; y != textureHeight; ++y)
			for (int x = 0; x != textureWidth; ++x)
			{
				int i = (x * 4) + (y * textureWidth * 4);
				if (x % 16 <= 7 && y % 16 <= 7)
				{
					textureData[i + 0] = 0;
					textureData[i + 1] = 0;
					textureData[i + 2] = 0;
					textureData[i + 3] = 0;
				}
				else
				{
					textureData[i + 0] = 255;
					textureData[i + 1] = 255;
					textureData[i + 2] = 255;
					textureData[i + 3] = 255;
				}
			}
			texture = device.CreateTexture2D(TextureFormat.B8G8R8A8, textureWidth, textureHeight, textureData, TextureMode.GPUOptimized);

			// create texture 2
			textureWidth = 100;
			textureHeight = 100;
			textureData = new byte[textureWidth * textureHeight * 4];
			for (int y = 0; y != textureHeight; ++y)
			for (int x = 0; x != textureWidth; ++x)
			{
				int i = (x * 4) + (y * textureWidth * 4);
				if (x % 16 <= 7 && y % 16 <= 7)
				{
					textureData[i + 0] = 0;
					textureData[i + 1] = 0;
					textureData[i + 2] = 0;
					textureData[i + 3] = 0;
				}
				else
				{
					textureData[i + 0] = 255;
					textureData[i + 1] = 255;
					textureData[i + 2] = 255;
					textureData[i + 3] = 255;
				}
			}
			texture2 = device.CreateTexture2D(TextureFormat.B8G8R8A8, textureWidth, textureHeight, textureData, TextureMode.GPUOptimized);

			// create constant buffer
			constantBufferObject = new ConstantBufferObject()
			{
				offset = .5f,
				constrast = .5f
			};
			constantBuffer = device.CreateConstantBuffer<ConstantBufferObject>(constantBufferObject, ConstantBufferMode.Write);

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
					usage = ShaderEffectResourceUsage.VS
				};
				desc.textures = new ShaderEffectTexture[2];
				desc.textures[0] = new ShaderEffectTexture()
				{
					registerIndex = 0,
					usage = ShaderEffectResourceUsage.PS
				};
				desc.textures[1] = new ShaderEffectTexture()
				{
					registerIndex = 1,
					usage = ShaderEffectResourceUsage.PS
				};
				desc.samplers = new ShaderEffectSampler[1];
				desc.samplers[0] = new ShaderEffectSampler()
				{
					registerIndex = 0,
					filter = ShaderEffectSamplerFilter.Default,
					anisotropy = ShaderEffectSamplerAnisotropy.Default,
					addressU = ShaderEffectSamplerAddress.Wrap,
					addressV = ShaderEffectSamplerAddress.Wrap,
					addressW = ShaderEffectSamplerAddress.Wrap
				};
				shaderEffect = device.CreateShaderEffect(vs, ps, null, null, null, desc, true);
			}

			// create vertex buffer
			var vertexBufferLayout = new VertexBufferLayout();
			vertexBufferLayout.elements = new VertexBufferLayoutElement[3];
			vertexBufferLayout.elements[0] = new VertexBufferLayoutElement()
			{
				type = VertexBufferLayoutElementType.Float3,
				usage = VertexBufferLayoutElementUsage.Position,
				usageIndex = 0,
				streamType = VertexBufferLayoutStreamType.VertexData,
				streamIndex = 0,
				streamByteOffset = 0
			};
			vertexBufferLayout.elements[1] = new VertexBufferLayoutElement()
			{
				type = VertexBufferLayoutElementType.RGBAx8,
				usage = VertexBufferLayoutElementUsage.Color,
				usageIndex = 0,
				streamType = VertexBufferLayoutStreamType.VertexData,
				streamIndex = 0,
				streamByteOffset = (sizeof(float) * 3)
			};
			vertexBufferLayout.elements[2] = new VertexBufferLayoutElement()
			{
				type = VertexBufferLayoutElementType.Float2,
				usage = VertexBufferLayoutElementUsage.UV,
				usageIndex = 0,
				streamType = VertexBufferLayoutStreamType.VertexData,
				streamIndex = 0,
				streamByteOffset = (sizeof(float) * 3) + 4
			};
			
			var vertices = new Vertex[]
			{
				new Vertex(new Vec3(-1, -1, 0), Color4.red, new Vec2(0, 0)),
				new Vertex(new Vec3(0, 1, 0), Color4.green, new Vec2(.5f, 1)),
				new Vertex(new Vec3(1, -1, 0), Color4.blue, new Vec2(1, 0))
			};
			vertexBuffer = device.CreateVertexBuffer<Vertex>(vertices, vertexBufferLayout, VertexBufferMode.GPUOptimized);

			// create index buffer
			var indices = new ushort[]
			{
				0, 1, 2
			};
			indexBuffer = device.CreateIndexBuffer(indices, IndexBufferMode.GPUOptimized);

			// create render state
			var renderStateDesc = new RenderStateDesc()
			{
				renderPass = renderPass,
				shaderEffect = shaderEffect,
				constantBuffers = new ConstantBufferBase[1],
				textures = new TextureBase[2],
				vertexBuffer = vertexBuffer,
				indexBuffer = indexBuffer,
				vertexBufferTopology = VertexBufferTopology.Triangle
			};
			renderStateDesc.constantBuffers[0] = constantBuffer;
			renderStateDesc.textures[0] = texture;
			renderStateDesc.textures[1] = texture2;
			renderState = device.CreateRenderState(renderStateDesc, 0);

			// print all GPUs this abstraction supports
			if (!instance.QuerySupportedAdapters(false, out var adapters)) throw new Exception("Failed: QuerySupportedAdapters");
			foreach (var adapter in adapters) Debug.WriteLine(adapter.name);
		}

		public void Dispose()
		{
			if (texture != null)
			{
				texture.Dispose();
				texture = null;
			}

			if (texture2 != null)
			{
				texture2.Dispose();
				texture2 = null;
			}

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

			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
				indexBuffer = null;
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
				commandList.Draw();
				commandList.EndRenderPass();
				commandList.Finish();
				commandList.Execute();
				device.EndFrame();

				// update constant buffer
				constantBufferObject.offset = (float)Math.Sin(rot);
				rot += 0.1f;
				constantBuffer.Update(constantBufferObject);

				// run application events
				application.RunEvents();
			}
		}
	}
}
