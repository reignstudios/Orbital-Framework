﻿using System;
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

		public Vertex Transform(Mat3 matrix, int repeat)
		{
			var result = this;
			for (int i = 0; i != repeat; ++i) result.position = result.position.Transform(matrix);
			return result;
		}
	}

	class RenderTextureTest
	{
		public Texture2DBase renderTexture;
		public RenderPassBase renderPass;
		public RenderStateBase renderState;
		public ShaderEffectBase shaderEffect;
		public VertexBufferBase vertexBuffer;
		public VertexBufferStreamerBase vertexBufferStreamer;

		public RenderTextureTest(DeviceBase device)
		{
			// create render texture
			const int size = 256;
			renderTexture = device.CreateRenderTexture2D(size, size, TextureFormat.Default, RenderTextureUsage.Discard, TextureMode.GPUOptimized, MSAALevel.Disabled, true, MultiGPUNodeResourceVisibility.Self);

			// load shader effect
			using (var vsStream = new FileStream("Shaders\\Triangle_D3D12.vs", FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var psStream = new FileStream("Shaders\\Triangle_D3D12.ps", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var vs = new Video.D3D12.Shader((Video.D3D12.Device)device, ShaderType.VS);
				var ps = new Video.D3D12.Shader((Video.D3D12.Device)device, ShaderType.PS);
				if (!vs.Init(vsStream)) throw new Exception("Failed to init VS shader");
				if (!ps.Init(psStream)) throw new Exception("Failed to init PS shader");
				var desc = new ShaderEffectDesc();
				shaderEffect = device.CreateShaderEffect(vs, ps, null, null, null, desc, true);
			}

			// create vertex buffer
			var vertices = new Vec3[3]
			{
				new Vec3(-1, -1, 0),
				new Vec3(0, 1, 0),
				new Vec3(1, -1, 0)
			};
			vertexBuffer = device.CreateVertexBuffer<Vec3>(vertices, VertexBufferMode.GPUOptimized);

			// create vertex buffer streamer
			var vertexBufferStreamLayout = new VertexBufferStreamLayout()
			{
				descs = new VertexBufferStreamDesc[1],
				elements = new VertexBufferStreamElement[1]
			};
			vertexBufferStreamLayout.descs[0] = new VertexBufferStreamDesc()
			{
				vertexBuffer = vertexBuffer,
				type = VertexBufferStreamType.VertexData
			};
			vertexBufferStreamLayout.elements[0] = new VertexBufferStreamElement()
			{
				type = VertexBufferStreamElementType.Float3,
				usage = VertexBufferStreamElementUsage.Position,
				offset = 0
			};
			vertexBufferStreamer = device.CreateVertexBufferStreamer(vertexBufferStreamLayout);

			// create render pass
			var renderPassDesc = RenderPassDesc.CreateDefault(new Color4F(0, 0, 0, 0), 1);
			renderPass = renderTexture.CreateRenderPass(renderPassDesc);

			// create render state
			var renderStateDesc = new RenderStateDesc()
			{
				renderPass = renderPass,
				shaderEffect = shaderEffect,
				vertexBufferTopology = VertexBufferTopology.Triangle,
				vertexBufferStreamer = vertexBufferStreamer,
				triangleCulling = TriangleCulling.Back,
				triangleFillMode = TriangleFillMode.Solid
			};
			renderState = device.CreateRenderState(renderStateDesc);
		}

		public void Dispose()
		{
			if (renderState != null)
			{
				renderState.Dispose();
				renderState = null;
			}

			if (renderPass != null)
			{
				renderPass.Dispose();
				renderPass = null;
			}

			if (renderTexture != null)
			{
				renderTexture.Dispose();
				renderTexture = null;
			}

			if (shaderEffect != null)
			{
				shaderEffect.Dispose();
				shaderEffect = null;
			}

			if (vertexBuffer != null)
			{
				vertexBuffer.Dispose();
				vertexBuffer = null;
			}

			if (vertexBufferStreamer != null)
			{
				vertexBufferStreamer.Dispose();
				vertexBufferStreamer = null;
			}
		}
	}

	public sealed partial class Example : IDisposable
	{
		private InstanceBase videoInstance;
		private DeviceBase videoDevice;
		private RasterizeCommandListBase commandList;
		private ComputeCommandListBase commandList_Compute;
		private RenderPassBase renderPass;
		private RenderStateBase renderState;
		private ShaderEffectBase shaderEffect;
		private ShaderVariableMapping shaderEffectVar_Constrast, shaderEffectVar_Camera;
		private VertexBufferBase vertexBuffer;
		private VertexBufferStreamerBase vertexBufferStreamer;
		private ConstantBufferBase constantBuffer;
		private Texture2DBase texture, texture2;
		private Texture2DBase renderTextureMSAA;
		private RenderTextureTest renderTextureTest;
		private ComputeShaderBase computeShader;
		private ComputeStateBase computeState;

		private Camera camera;
		private float rot = .85f;

		private void InitVideo(string platformPath, string libFolderBit, string config)
		{
			// load api abstraction (api-instance and hardware-device)
			var abstractionDesc = new AbstractionDesc(AbstractionInitType.SingleGPU_Standard);
			abstractionDesc.supportedAPIs = new AbstractionAPI[]
			{
				AbstractionAPI.D3D12
			};

			//abstractionDesc.instanceDescD3D12.attachDebugger = true;
			//abstractionDesc.instanceDescD3D12.extraDebugging = true;
			abstractionDesc.deviceDescD3D12.window = window;
			//abstractionDesc.deviceDescD3D12.adapterIndex = 2;
			//abstractionDesc.deviceDescD3D12.vSyncMode = SwapChainVSyncMode.VSyncOff;
			#if DEBUG
			abstractionDesc.nativeLibPathD3D12 = Path.Combine(platformPath, @"Native\Orbital.Video.D3D12.Native\bin", libFolderBit, config);
			#else
			abstractionDesc.nativeLibPathD3D12 = string.Empty;
			#endif

			abstractionDesc.deviceDescVulkan.window = window;
			#if DEBUG
			abstractionDesc.nativeLibPathVulkan = Path.Combine(platformPath, @"Native\Orbital.Video.Vulkan.Native\bin", libFolderBit, config);
			#else
			abstractionDesc.nativeLibPathVulkan = string.Empty;
			#endif
			
			if (!Abstraction.InitFirstAvaliable(abstractionDesc, out videoInstance, out videoDevice)) throw new Exception("Failed to init video abstraction");
			
			// create render texture test objects
			renderTextureTest = new RenderTextureTest(videoDevice);
			
			// create msaa render texture
			if (!videoDevice.GetMaxMSAALevel(TextureFormat.Default, out var msaaLevel)) throw new Exception("Failed to get MSAA level");
			var windowSize = window.GetSize();
			renderTextureMSAA = videoDevice.CreateRenderTexture2D(windowSize.width, windowSize.height, TextureFormat.Default, RenderTextureUsage.Discard, TextureMode.GPUOptimized, StencilUsage.Discard, DepthStencilFormat.DefaultDepth, DepthStencilMode.GPUOptimized, msaaLevel, false, MultiGPUNodeResourceVisibility.All);

			// create command list
			commandList = videoDevice.CreateRasterizeCommandList();
			commandList_Compute = videoDevice.CreateComputeCommandList();

			// create render pass
			var renderPassDesc = RenderPassDesc.CreateDefault(new Color4F(0, .2f, .4f, 1), 1);
			//renderPass = device.CreateRenderPass(renderPassDesc, device.swapChain.depthStencil);
			renderPass = renderTextureMSAA.CreateRenderPass(renderPassDesc, renderTextureMSAA.GetDepthStencil());
			
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
			texture = videoDevice.CreateTexture2D(textureWidth, textureHeight, TextureFormat.R8G8B8A8, textureData, TextureMode.GPUOptimized, MultiGPUNodeResourceVisibility.Self);
			
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
			texture2 = videoDevice.CreateTexture2D(textureWidth, textureHeight, TextureFormat.R8G8B8A8, textureData, TextureMode.GPUOptimized, MultiGPUNodeResourceVisibility.Self);
			
			// load shaders
			// TODO: load CS2X compiled ShaderEffect
			/*using (var stream = new FileStream("Shader.se", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				shaderEffect = device.CreateShaderEffect(stream, ShaderEffectSamplerAnisotropy.Default);
			}*/

			using (var vsStream = new FileStream("Shaders\\Shader_D3D12.vs", FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var psStream = new FileStream("Shaders\\Shader_D3D12.ps", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var vs = new Video.D3D12.Shader((Video.D3D12.Device)videoDevice, ShaderType.VS);
				var ps = new Video.D3D12.Shader((Video.D3D12.Device)videoDevice, ShaderType.PS);
				if (!vs.Init(vsStream)) throw new Exception("Failed to init VS shader");
				if (!ps.Init(psStream)) throw new Exception("Failed to init PS shader");
				var desc = new ShaderEffectDesc();
				desc.constantBuffers = new ShaderEffectConstantBuffer[1];
				desc.constantBuffers[0] = new ShaderEffectConstantBuffer()
				{
					registerIndex = 0,
					usage = ShaderEffectResourceUsage.VS,
					variables = new ShaderVariable[2]
				};
				desc.constantBuffers[0].variables[0] = new ShaderVariable()
				{
					name = "constrast",
					type = ShaderVariableType.Float
				};
				desc.constantBuffers[0].variables[1] = new ShaderVariable()
				{
					name = "camera",
					type = ShaderVariableType.Float4x4
				};
				desc.textures = new ShaderEffectTexture[3];
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
				desc.textures[2] = new ShaderEffectTexture()
				{
					registerIndex = 2,
					usage = ShaderEffectResourceUsage.PS
				};
				desc.samplers = new ShaderEffectSampler[1];
				desc.samplers[0] = new ShaderEffectSampler()
				{
					registerIndex = 0,
					filter = ShaderSamplerFilter.Default,
					anisotropy = ShaderSamplerAnisotropy.Default,
					addressU = ShaderSamplerAddress.Wrap,
					addressV = ShaderSamplerAddress.Wrap,
					addressW = ShaderSamplerAddress.Wrap,
					usage = ShaderEffectResourceUsage.PS
				};
				shaderEffect = videoDevice.CreateShaderEffect(vs, ps, null, null, null, desc, true);
			}

			if (!shaderEffect.FindVariable("constrast", out shaderEffectVar_Constrast)) throw new Exception("Failed to find shader effect variable");
			if (!shaderEffect.FindVariable("camera", out shaderEffectVar_Camera)) throw new Exception("Failed to find shader effect variable");

			// create constant buffer
			constantBuffer = videoDevice.CreateConstantBuffer(shaderEffect.constantBufferMappings[0].size, ConstantBufferMode.Write);

			// create vertex buffer
			const float size = 1 / 2f;
			var rotUpAxisMat = Mat3.FromEuler(0, MathTools.DegToRad(90), 0);
			var rotRightAxisMat = Mat3.FromEuler(MathTools.DegToRad(90), 0, 0);
			var vertices = new Vertex[4 * 6];// 4 vertices per face
			var indices = new ushort[6 * 6];// 6 indices per face
			var colorKey = new Color4[4]
			{
				Color4.blue,
				Color4.red,
				Color4.white,
				Color4.white
			};
			for (int v = 0, i = 0, r = 0; v < (4 * 4); v += 4, i += 6, ++r)// caluclate front, right, back, left faces
			{
				vertices[v + 0] = new Vertex(new Vec3(-size, -size, size), colorKey[r], new Vec2(0, 0)).Transform(rotUpAxisMat, r);
				vertices[v + 1] = new Vertex(new Vec3(-size, size, size), colorKey[r], new Vec2(0, 1)).Transform(rotUpAxisMat, r);
				vertices[v + 2] = new Vertex(new Vec3(size, size, size), colorKey[r], new Vec2(1, 1)).Transform(rotUpAxisMat, r);
				vertices[v + 3] = new Vertex(new Vec3(size, -size, size), colorKey[r], new Vec2(1, 0)).Transform(rotUpAxisMat, r);
				indices[i + 0] = (ushort)(v + 0);
				indices[i + 1] = (ushort)(v + 1);
				indices[i + 2] = (ushort)(v + 2);
				indices[i + 3] = (ushort)(v + 0);
				indices[i + 4] = (ushort)(v + 2);
				indices[i + 5] = (ushort)(v + 3);
			}
			colorKey = new Color4[2]
			{
				Color4.green,
				Color4.white
			};
			for (int v = (4 * 4), i = (6 * 4), r = 1; v < (4 * 6); v += 4, i += 6, r = 3)// caluclate top, bottom faces
			{
				vertices[v + 0] = new Vertex(new Vec3(-size, -size, size), colorKey[r / 3], new Vec2(0, 0)).Transform(rotRightAxisMat, r);
				vertices[v + 1] = new Vertex(new Vec3(-size, size, size), colorKey[r / 3], new Vec2(0, 1)).Transform(rotRightAxisMat, r);
				vertices[v + 2] = new Vertex(new Vec3(size, size, size), colorKey[r / 3], new Vec2(1, 1)).Transform(rotRightAxisMat, r);
				vertices[v + 3] = new Vertex(new Vec3(size, -size, size), colorKey[r / 3], new Vec2(1, 0)).Transform(rotRightAxisMat, r);
				indices[i + 0] = (ushort)(v + 0);
				indices[i + 1] = (ushort)(v + 1);
				indices[i + 2] = (ushort)(v + 2);
				indices[i + 3] = (ushort)(v + 0);
				indices[i + 4] = (ushort)(v + 2);
				indices[i + 5] = (ushort)(v + 3);
			}
			vertexBuffer = videoDevice.CreateVertexBuffer<Vertex>(vertices, indices, VertexBufferMode.GPUOptimized);

			// create vertex buffer streamer
			var vertexBufferStreamLayout = new VertexBufferStreamLayout()
			{
				descs = new VertexBufferStreamDesc[1],
				elements = new VertexBufferStreamElement[3]
			};
			vertexBufferStreamLayout.descs[0] = new VertexBufferStreamDesc()
			{
				vertexBuffer = vertexBuffer,
				type = VertexBufferStreamType.VertexData
			};
			vertexBufferStreamLayout.elements[0] = new VertexBufferStreamElement()
			{
				type = VertexBufferStreamElementType.Float3,
				usage = VertexBufferStreamElementUsage.Position,
				offset = 0
			};
			vertexBufferStreamLayout.elements[1] = new VertexBufferStreamElement()
			{
				type = VertexBufferStreamElementType.RGBAx8,
				usage = VertexBufferStreamElementUsage.Color,
				offset = (sizeof(float) * 3)
			};
			vertexBufferStreamLayout.elements[2] = new VertexBufferStreamElement()
			{
				type = VertexBufferStreamElementType.Float2,
				usage = VertexBufferStreamElementUsage.UV,
				offset = (sizeof(float) * 3) + 4
			};
			vertexBufferStreamer = videoDevice.CreateVertexBufferStreamer(vertexBufferStreamLayout);

			// create render state
			var renderStateDesc = new RenderStateDesc()
			{
				renderPass = renderPass,
				shaderEffect = shaderEffect,
				constantBuffers = new ConstantBufferBase[1],
				textures = new TextureBase[3],
				vertexBufferTopology = VertexBufferTopology.Triangle,
				vertexBufferStreamer = vertexBufferStreamer,
				triangleCulling = TriangleCulling.Back,
				triangleFillMode = TriangleFillMode.Solid,
				depthStencilDesc = DepthStencilDesc.StandardDepthTesting()
			};
			//renderStateDesc.blendDesc.renderTargetBlendDescs = new RenderTargetBlendDesc[1] {RenderTargetBlendDesc.AlphaBlending()};
			renderStateDesc.constantBuffers[0] = constantBuffer;
			renderStateDesc.textures[0] = texture;
			renderStateDesc.textures[1] = texture2;
			renderStateDesc.textures[2] = renderTextureTest.renderTexture;
			renderState = videoDevice.CreateRenderState(renderStateDesc);

			// create compute shader
			using (var csStream = new FileStream("Shaders\\Compute_D3D12.cs", FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var csDesc = new ComputeShaderDesc()
				{
					randomAccessBuffers = new ComputeShaderRandomAccessBuffer[1]
				};
				csDesc.randomAccessBuffers[0] = new ComputeShaderRandomAccessBuffer()
				{
					registerIndex = 0
				};
				computeShader = videoDevice.CreateComputeShader(csStream, csDesc);
			}

			// create compute state
			var computeStateDesc = new ComputeStateDesc()
			{
				computeShader = computeShader,
				randomAccessBuffers = new object[1]
			};
			computeStateDesc.randomAccessBuffers[0] = renderTextureTest.renderTexture;
			computeState = videoDevice.CreateComputeState(computeStateDesc);

			// print all GPUs this abstraction supports
			if (!videoInstance.QuerySupportedAdapters(false, out var adapters)) throw new Exception("Failed: QuerySupportedAdapters");
			foreach (var adapter in adapters) Log(adapter.name);

			// setup camera
			camera = new Camera();
		}

		private void DisposeVideo()
		{
			if (computeState != null)
			{
				computeState.Dispose();
				computeState = null;
			}

			if (computeShader != null)
			{
				computeShader.Dispose();
				computeShader = null;
			}

			if (renderTextureMSAA != null)
			{
				renderTextureMSAA.Dispose();
				renderTextureMSAA = null;
			}

			if (renderTextureTest != null)
			{
				renderTextureTest.Dispose();
				renderTextureTest = null;
			}

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

			if (vertexBufferStreamer != null)
			{
				vertexBufferStreamer.Dispose();
				vertexBufferStreamer = null;
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

			if (commandList_Compute != null)
			{
				commandList_Compute.Dispose();
				commandList_Compute = null;
			}

			if (videoDevice != null)
			{
				videoDevice.Dispose();
				videoDevice = null;
			}

			if (videoInstance != null)
			{
				videoInstance.Dispose();
				videoInstance = null;
			}
		}
	}
}